using Intellidesk.Data.Auth.Providers;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler.Encoder;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json.Serialization;
using Owin;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http.Formatting;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;

namespace Intellidesk.Data.Auth
{
    public static class IdentityExtensions
    {
        public static OAuthAuthorizationServerOptions OAuthOptions { get; private set; }
        public static string PublicClientId { get; private set; }

        public static void UseJwtBearerAuthentication(this IAppBuilder app)
        {
            var issuer = ConfigurationManager.AppSettings["as:AudienceId"];
            string audienceId = ConfigurationManager.AppSettings["as:AudienceId"];
            byte[] audienceSecret = TextEncodings.Base64Url.Decode(ConfigurationManager.AppSettings["as:AudienceSecret"]);

            // Api controllers with an [Authorize] attribute will be validated with JWT
            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    AllowedAudiences = new[] { audienceId },
                    IssuerSecurityKeyProviders = new IIssuerSecurityKeyProvider[]
                    {
                        new SymmetricKeyIssuerSecurityKeyProvider(issuer, audienceSecret)
                    }
                });
        }

        public static void UseOAuthBearerTokens(this IAppBuilder app)
        {
            // Configure the application for OAuth based flow
            PublicClientId = "self";
            OAuthOptions = new OAuthAuthorizationServerOptions
            {
                TokenEndpointPath = new PathString("/Token"),
                Provider = new AppOAuthProvider(PublicClientId),
                AuthorizeEndpointPath = new PathString("/api/Account/ExternalLogin"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(14),
                // In production mode set AllowInsecureHttp = false
                AllowInsecureHttp = true,
            };

            // Enable the application to use bearer tokens to authenticate users
            app.UseOAuthBearerTokens(OAuthOptions);
            app.UseJwtBearerAuthentication();
        }

        public static void UseAuthentication_Sample(this IAppBuilder app)
        {
            var oauthProvider = new OAuthAuthorizationServerProvider
            {
                OnGrantResourceOwnerCredentials = context =>
                {
                    if (context.UserName == "admin" && context.Password == "123456")
                    {
                        var claimsIdentity = new ClaimsIdentity(context.Options.AuthenticationType);
                        claimsIdentity.AddClaim(new Claim("user", context.UserName));
                        context.Validated(claimsIdentity);
                        return Task.CompletedTask;
                    }
                    context.Rejected();
                    return Task.CompletedTask;
                },
                OnValidateClientAuthentication = context =>
                {
                    string clientId;
                    string clientSecret;
                    if (context.TryGetBasicCredentials(out clientId, out clientSecret))
                    {
                        if (clientId == "admin" && clientSecret == "secretKey")
                        {
                            context.Validated();
                        }
                    }
                    return Task.CompletedTask;
                }
            };
            //var oauthOptions = new OAuthAuthorizationServerOptions
            //{
            //    AllowInsecureHttp = true,
            //    TokenEndpointPath = new PathString("/api/Account/token"),
            //    Provider = oauthProvider,
            //    AuthorizationCodeExpireTimeSpan = TimeSpan.FromMinutes(1),
            //    AccessTokenExpireTimeSpan = TimeSpan.FromDays(3),
            //    SystemClock = new SystemClock()

            //};
            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/api/Account/token"),
                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
                Provider = new SimpleAuthorizationServerProvider()
            };

            // Token Generation
            app.UseOAuthAuthorizationServer(OAuthServerOptions);
            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

            // Uncomment the following lines to enable logging in with third party login providers
            //app.UseMicrosoftAccountAuthentication(
            //    clientId: "",
            //    clientSecret: "");

            //app.UseTwitterAuthentication(
            //    consumerKey: "",
            //    consumerSecret: "");

            //app.UseFacebookAuthentication(
            //    appId: "",
            //    appSecret: "");

            //app.UseGoogleAuthentication(new GoogleOAuth2AuthenticationOptions()
            //{
            //    ClientId = "",
            //    ClientSecret = ""
            //});
        }

        public static IAppBuilder UseJsonWebToken(this IAppBuilder app, string issuer, string audience, string audienceSecret, IOAuthBearerAuthenticationProvider location)
        {
            if (app == null)
                throw new ArgumentNullException("app");

            var options = new JwtBearerAuthenticationOptions
            {
                AllowedAudiences = new[] { audience },
                IssuerSecurityKeyProviders = new[]
                    {
                        new SymmetricKeyIssuerSecurityKeyProvider(issuer, audienceSecret) //signingKey
                    }
            };

            if (location != null)
                options.Provider = location;

            app.UseJwtBearerAuthentication(options);

            return app;
        }

        /// <summary>
        /// Adds JWT bearer token middleware to your web application pipeline.
        /// </summary>
        /// <param name="app">The IAppBuilder passed to your configuration method.</param>
        /// <param name="options">An options class that controls the middleware behavior.</param>
        /// <returns>The original app parameter.</returns>
        public static IAppBuilder UseJwtBearerAuthentication(this IAppBuilder app, JwtBearerAuthenticationOptions options)
        {
            if (app == null)
            {
                throw new ArgumentNullException("app");
            }
            if (options == null)
            {
                throw new ArgumentNullException("options");
            }

            var bearerOptions = new OAuthBearerAuthenticationOptions
            {
                Realm = options.Realm,
                Provider = options.Provider,
                //AccessTokenFormat = new JwtFormat(options.AllowedAudiences.ToString(), options.IssuerSecurityKeyProviders),
                AuthenticationMode = options.AuthenticationMode,
                AuthenticationType = options.AuthenticationType,
                Description = options.Description
            };

            app.UseOAuthBearerAuthentication(bearerOptions);

            return app;
        }

        //public JwtBearerTokenAuthenticationOptions(this IAppBuilder app, JwtOptions jwtOptions)
        //{
        //    if (jwtOptions == null)
        //    {
        //        throw new ArgumentNullException("jwtOptions");
        //    }

        //    byte[] symmetricKeyBytes = Encoding.UTF8.GetBytes(jwtOptions.JwtSigningKeyAsUtf8);
        //    string symmetricKeyAsBase64 = Convert.ToBase64String(symmetricKeyBytes);

        //    var symmetricKeyIssuerSecurityTokenProvider = new SymmetricKeyIssuerSecurityTokenProvider(
        //        jwtOptions.Issuer, symmetricKeyAsBase64);

        //    var providers = new IIssuerSecurityTokenProvider[]
        //                    {
        //                        symmetricKeyIssuerSecurityTokenProvider
        //                    };

        //    _jwtBearerOptions = new JwtBearerAuthenticationOptions
        //    {
        //        AllowedAudiences = new List<string> { jwtOptions.Audience },
        //        IssuerSecurityTokenProviders = providers
        //    };

        //    _jwtOptions = jwtOptions;
        //}

        public static void UseJsonFormat(this IAppBuilder app, HttpConfiguration config)
        {
            var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            jsonFormatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        }
    }
}
