using AutoMapper;
using FluentValidation;
using FluentValidation.AspNetCore;
using ID.Api.Interfaces;
using ID.Api.Services;
using ID.Infrastructure.Auth;
using ID.Infrastructure.Core;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Filters;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Middleware;
using ID.Infrastructure.Models;
using ID.WebApi.Models.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ID.Api
{
    public class Startup
    {
        private readonly IConfiguration Configuration;

        public Startup(IConfiguration config)
        {
            Configuration = config;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
            });

            services.Configure<KestrelServerOptions>(Configuration.GetSection("Kestrel"));

            services.AddCors(o => o.AddPolicy("ApiCorsPolicy", builder =>
            {
                builder
                .AllowAnyMethod()
                .AllowAnyHeader()
                .AllowAnyOrigin();
            }));

            // configurations
            services.AddSingleton(Configuration);
            services.AddOptions(); //Add Support for strongly typed Configuration and map to class

            // appConfig and authOptions
            var configSection = Configuration.GetSection("AppConfig");
            var appConfig = configSection?.Get<AppConfig>();
            services.AddSingleton<IAppConfig>(appConfig);

            // authOptions
            var authConfigSection = Configuration.GetSection("AuthOptions");
            var authOptions = authConfigSection?.Get<AuthOptions>();
            services.AddSingleton<IAuthOptions>(authOptions);

            // healths
            services.AddHealthChecks();

            // contexts
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IActionContextAccessor, ActionContextAccessor>();

            // REST http clients
            services.AddHttpClient<GeneralHttpClient>("DalApi",
                httpClient =>
                {
                    var endpoint = Configuration.GetSection("AppConfig:Endpoints:DalApi").Value;
                    httpClient.BaseAddress = new Uri(endpoint);
                    httpClient.DefaultRequestHeaders.Add("User-Agent", appConfig.Domain);
                    httpClient.DefaultRequestHeaders.Add("X-Named-Client", "DalApi");
                    httpClient.DefaultRequestHeaders.AddFromRequest("Authorization");
                    var isCustomTimeout = double.TryParse(appConfig.ResponseTimeout, out double apiResponseTimeout);
                    httpClient.Timeout = TimeSpan.FromMinutes(isCustomTimeout ? apiResponseTimeout : 10);
                });

            // Authorization
            services.AddAuthorization(options =>
            {
                options.AddPolicy(PolicyTypes.ApiAuthPolicy, policy =>
                    policy.Requirements.Add(new TokenRequirement(authOptions)));
            });
            services.AddSingleton<IAuthorizationHandler, ApiAuthorizationHandler>();

            // Current referenced assemblies for search validators and mappers
            List<Assembly> referencedAssemblies = Assembly.GetEntryAssembly().GetReferencedAssemblies()
                .Where(x => x.Name.ToUpper().Contains("CRPM"))
                .Select(x => Assembly.Load(x.FullName))
                .ToList();
            referencedAssemblies.Add(Assembly.GetEntryAssembly());

            // mvc
            services
                .AddMvc(options =>
                {
                    options.Filters.Add(typeof(ApiExceptionFilter));
                    options.Filters.Add(typeof(ApiValidationFilter));
                    //options.ModelBinderProviders.Insert(0, new GuidHeaderModelBinderProvider());
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddFluentValidation(config =>
                {
                    config.RegisterValidatorsFromAssemblies(referencedAssemblies);
                    config.RunDefaultMvcValidationAfterFluentValidationExecutes = false;
                });

            // model state validation
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            services.AddControllers()
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                    options.UseMemberCasing();
                })
                .AddJsonOptions(jsonOptions =>
                {
                    jsonOptions.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
                });

            // authentication
            services.ConfigureApiAuthentication(authOptions);

            services.AddMemoryCache();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = $"{Assembly.GetEntryAssembly().GetName().Name}", Version = "v1" });

                if (authOptions.AuthenticationType == "Bearer")
                {
                    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                    {
                        Description = @"JWT Authorization header, example: 'Bearer xxxxx.xxxxx.xxxxx",
                        Name = "Authorization",
                        In = ParameterLocation.Header,
                        Type = SecuritySchemeType.ApiKey
                    });
                    c.OperationFilter<ApiAuthorizationOperationFilter>();
                    c.OperationFilter<ApiSwaggerOperationNameFilter>();

                    c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                      {
                            {
                              new OpenApiSecurityScheme
                              {
                                Reference = new OpenApiReference
                                  {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                  }
                                },
                                new List<string>()
                              }
                        });
                }
            });

            // automapper
            services.AddAutoMapper(referencedAssemblies);

            // logic services
            //services.AddScoped<OrganizationService>();
            //services.AddScoped<ActivityService>();
            //services.AddScoped<ReportService>();
            //services.AddScoped<ActivityTemplateService>();
            //services.AddScoped<FormService>();
            //services.AddScoped<FormTemplateService>();
            //services.AddScoped<GeneralService>();
            //services.AddScoped<ModelService>();
            services.AddScoped<IUserService, UserService>();

            services.AddScoped(provider =>
            {
                var connectionString = Configuration.GetConnectionString("DbConn");
                var builder = new DbContextOptionsBuilder<IdentityContext>();
                var env = provider.GetService<IWebHostEnvironment>();

                // migrations
                var migrationsAssemblyName = env.IsProduction() || env.IsDevelopment()
                    ? Assembly.GetEntryAssembly().GetName().Name
                    : $"{typeof(Startup).Assembly.GetName().Name}.{env.EnvironmentName}";

                builder.UseNpgsql(connectionString, x => x.MigrationsAssembly(migrationsAssemblyName));
#if DEBUG
                //log for db queries, use serilog class implementing ILoggerFactory
                builder.UseLoggerFactory(provider.GetService<ILoggerFactory>());
#endif
                var dbContext = new IdentityContext(builder.Options);
                return dbContext;
            });

            //services.AddTransient<IValidator<UserDetails>, UserDetailsValiator>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, ILoggerFactory loggerFactory)
        {
            GeneralContext.SetServiceProvider(app.ApplicationServices);
            app.UseResponseCompression();

            // migrations
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var dbContext = serviceScope.ServiceProvider.GetService<IdentityContext>();

                // ensure create database
                GeneralContext.Logger.Information("Database creating...");
                var isCreated = dbContext.Database.EnsureCreated();
                GeneralContext.Logger.Information(isCreated ? "Database created successfully" : "Database already existed");

                var currentMigrations = dbContext.Database.GetMigrations();
                var pendingMigrations = dbContext.Database.GetPendingMigrations();

                // apply pending migrations
                if (pendingMigrations.Any())
                {
                    var migrationNames = pendingMigrations.ToList();
                    try
                    {
                        dbContext.Database.Migrate();

                        pendingMigrations = dbContext.Database.GetPendingMigrations();

                        if (!pendingMigrations.Any())
                            GeneralContext.Logger.Information($"Migrations with {string.Join(",", migrationNames)} applied successfully");
                        else
                            GeneralContext.Logger.Warning($"Migrations with {string.Join(", ", migrationNames)} not applied!");
                    }
                    catch (Exception ex)
                    {
                        GeneralContext.LastErrors.Add(ex);
                    }
                }
            }

            app.UseHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    Dictionary<string, object> checks = new Dictionary<string, object>();

                    if (report.Entries.Count > 0)
                    {
                        checks = report.Entries.Values.FirstOrDefault()
                            .Data.ToDictionary(x => x.Key, x => x.Value);
                    }

                    checks.Add("Endpoint", context.Request.Scheme + Uri.SchemeDelimiter + context.Request.Host.Value);

                    var settings = new JsonSerializerSettings();
                    settings.Converters.Add(new StringEnumConverter());
                    settings.Converters.Add(new IsoDateTimeConverter { DateTimeFormat = "dd/MM/yyyy" });

                    await context.Response.WriteAsync(
                        JsonConvert.SerializeObject(HealthCheckResult.Healthy($"{Assembly.GetEntryAssembly().GetName()}", checks), Formatting.Indented, settings));
                }
            });

            app.UseMiddleware<ApiErrorHandlingMiddleware>();
            app.UseMiddleware<ApiAuthenticationMiddleware>();
            app.UseMiddleware<ApiContextMiddleware>();

            //app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseCors("ApiCorsPolicy");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", $"{Assembly.GetEntryAssembly().GetName().Name}");
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapGet("/", context =>
                {
                    context.Response.Redirect("swagger/index.html");
                    return Task.CompletedTask;
                });
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }

    public class GuidHeaderModelBinderProvider : IModelBinderProvider
    {
        public IModelBinder GetBinder(ModelBinderProviderContext Context)
        {
            if (Context.Metadata.ModelType == typeof(Guid))
            {
                if (Context.BindingInfo.BindingSource == BindingSource.Header)
                {
                    return new BinderTypeModelBinder(typeof(GuidHeaderModelBinder));
                }
            }
            return null;
        }
    }

    public class GuidHeaderModelBinder : IModelBinder
    {
        public async Task BindModelAsync(ModelBindingContext BindingContext)
        {
            // Read HTTP header.
            string headerName = BindingContext.FieldName;
            if (BindingContext.HttpContext.Request.Headers.ContainsKey(headerName))
            {
                StringValues headerValues = BindingContext.HttpContext.Request.Headers[headerName];
                if (headerValues == StringValues.Empty)
                {
                    // Value not found in HTTP header.  Substitute empty GUID.
                    BindingContext.ModelState.SetModelValue(BindingContext.FieldName, headerValues, Guid.Empty.ToString());
                    BindingContext.Result = ModelBindingResult.Success(Guid.Empty);
                }
                else
                {
                    // Value found in HTTP header.
                    string correlationIdText = headerValues[0];
                    BindingContext.ModelState.SetModelValue(BindingContext.FieldName, headerValues, correlationIdText);
                    // Parse GUID.
                    BindingContext.Result = Guid.TryParse(correlationIdText, out Guid correlationId)
                        ? ModelBindingResult.Success(correlationId)
                        : ModelBindingResult.Failed();
                }
            }
            else
            {
                // HTTP header not found.
                BindingContext.Result = ModelBindingResult.Failed();
            }
            await Task.FromResult(default(object));
        }
    }
}
