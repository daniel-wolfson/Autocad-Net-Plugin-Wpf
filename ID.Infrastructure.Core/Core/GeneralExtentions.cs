using ID.Infrastructure.Auth;
using ID.Infrastructure.DataContext;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Helpers;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace ID.Infrastructure.Core
{
    /// <summary> Configure services; Use app middlewares </summary>
    public static class GeneralExtentions
    {
        public static void ConfigureRestClients(this IServiceCollection services, AppConfig appConfig)
        {
            //services.AddTransient<AuthTokenHandler>();
            // httpClients
            foreach (KeyValuePair<string, string> endpoint in appConfig.Endpoints)
            {
                services.AddHttpClient<GeneralHttpClient>(endpoint.Key,
                    httpClient =>
                    {
                        httpClient.BaseAddress = new Uri(endpoint.Value);
                        httpClient.DefaultRequestHeaders.Add("User-Agent", appConfig.Domain);
                        httpClient.DefaultRequestHeaders.Add("X-Named-Client", endpoint.Key);
                        httpClient.DefaultRequestHeaders.AddFromRequest("Authorization");
                        httpClient.DefaultRequestHeaders.AddFromRequest("Accept-Language");
                        var isCustomTimeout = double.TryParse(appConfig.ResponseTimeout, out double apiResponseTimeout);
                        httpClient.Timeout = TimeSpan.FromMinutes(isCustomTimeout ? apiResponseTimeout : 10);

                    });
                //.AddHttpMessageHandler<AuthTokenHandler>();
                //.AddHttpMessageHandler<HttpClientDelegatingHandler>();
            }
        }

        public static IConfigurationRoot ConfigureApiEnvironment(this IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();

            return builder.Build();
        }

        public static void ConfigureApiAuthentication(this IServiceCollection services, IAuthOptions authOptions)
        {
            AuthenticationBuilder authenticationBuilder;
            if (authOptions.AuthenticationType == "Bearer")
            {
                authenticationBuilder = services
                    .AddAuthentication(x =>
                    {
                        x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                        //x.RequireAuthenticatedSignIn = false;
                    })
                    .AddJwtBearer(options =>
                    {
                        IAuthOptions authOptions = GeneralContext.GetService<IAuthOptions>();
                        var key = Encoding.ASCII.GetBytes(authOptions.KEY);

                        options.Events = ConfigureJwtBearerEvents();

                        options.RequireHttpsMetadata = false;
                        options.SaveToken = true;
                        options.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(key),
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            RequireExpirationTime = false,
                            ValidateLifetime = true
                        };
                        //options.SecurityTokenValidators.Add(new ApiTokenValidator());
                    });
            }
            else
            {
                authenticationBuilder = services.AddAuthentication(ApiAuthSchemes.DefaultAuthScheme);
            }

            authenticationBuilder.AddCookie(ApiAuthSchemes.DefaultAuthScheme, options =>
            {
                options.LoginPath = new Microsoft.AspNetCore.Http.PathString("/AdminApi/Login");
            });
        }

        public static void ConfigureApiAuthorization(this IServiceCollection services, IAuthOptions authOptions)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyTypes.ApiAuthPolicy, policy =>
                    policy.Requirements.Add(new TokenRequirement(authOptions)));

                //options =>
                //{
                //    options.AddPolicy("Over18", policy =>
                //    {
                //        policy.AuthenticationSchemes.Add("CrpmAuthenticationScheme");
                //        policy.RequireAuthenticatedUser();
                //        //policy.Requirements.Add(new MinimumAgeRequirement());
                //    });
                //});
            });
            services.AddSingleton<IAuthorizationHandler, ApiAuthorizationHandler>();

        }

        public static JwtBearerEvents ConfigureJwtBearerEvents()
        {
            return new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
                    {
                        context.Response.Headers.Add("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    // Add the access_token as a claim, as we may actually need it
                    var accessToken = context.SecurityToken as JwtSecurityToken;
                    if (accessToken != null)
                    {
                        ClaimsIdentity identity = context.Principal.Identity as ClaimsIdentity;
                        if (identity != null)
                        {
                            identity.AddClaim(new Claim("access_token", accessToken.RawData));
                            IAuthOptions authOptions = GeneralContext.GetService<IAuthOptions>();
                            AppUser appUser = Util.ReadToken<AppUser>(accessToken.RawData, authOptions.KEY);

                            if (appUser != null)
                                context.Success();
                            else
                                context.Fail("Unauthorized");
                        }
                    }

                    if (!context.Result.Succeeded)
                    {
                        context.Response.Headers.Add("Token-OnTokenValidated", "false");
                    }

                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    if (context.Response.StatusCode < 200 || context.Response.StatusCode > 299)
                    {
                        context.Response.Headers.Add("Token-OnChallenge", "false");
                    }
                    return Task.CompletedTask;
                },

            };
        }

        public static void ConfigureUnitOfWork<T>(this IServiceCollection services) where T : IDbContext
        {
            services.AddScoped<IUnitOfWork<T>, UnitOfWork<T>>();
            services.AddScoped<IUnitOfWork>(provider => provider.GetService<IUnitOfWork<T>>());
        }

        public static void ConfigureUnitOfWorkPool(this IServiceCollection services, Action<UnitOfWorkPoolOptionsBuilder> optionsAction)
        {
            var optionsBuilder = new UnitOfWorkPoolOptionsBuilder();
            optionsAction.Invoke(optionsBuilder);

            services.AddScoped(typeof(IUnitOfWork<>), typeof(UnitOfWork<>));
            services.AddSingleton(typeof(UnitOfWorkPoolOptions), optionsBuilder.Options);
            services.AddScoped<IUnitOfWorkPool, UnitOfWorkPool>();
        }

        public static IServiceScope ConfigureScope(this IServiceProvider provider)
        {
            return provider.GetRequiredService<IServiceScopeFactory>().CreateScope();
        }

        public static bool NotNullOrEmpty<T>(this IEnumerable<T> source)
        {
            return source != null && source.Any();
        }

        public static IEnumerable<T> GetFlattenList<T>(this IEnumerable<T> source) where T : class
        {
            return source.Concat(source.SelectMany(i => GetFlattenList<T>(i as IEnumerable<T>))); //i.children
        }
    }
}
