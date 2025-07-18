using ID.Infrastructure.Core;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Helpers;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ID.Infrastructure.Middleware
{
    public class ApiRequestMiddleware
    {
        private readonly RequestDelegate _next;

        public ApiRequestMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            IAuthOptions authOptions = GeneralContext.GetService<IAuthOptions>();
            var token = context.GetRequestHeader(HttpRequestHeader.Authorization);
            var loggedUser = Util.ReadToken<AppUser>(token, authOptions.KEY);

            //var loggedUser = JsonConvert.DeserializeObject<AppUser>(context.GetRequestHeader(HttpRequestXHeader.User));
            if (loggedUser != null && !string.IsNullOrEmpty(loggedUser.FirstName))
                loggedUser.FirstName = loggedUser.FirstName.ToStringUtf8();

            if (loggedUser != null && !string.IsNullOrEmpty(loggedUser.LastName))
                loggedUser.LastName = loggedUser.LastName.ToStringUtf8();

            context.Items.TryAdd(SessionKeys.AppUser.GetDisplayName(), loggedUser);

            var dataString = context.GetRequestHeader(HttpRequestXHeader.Data);
            if (!string.IsNullOrEmpty(dataString))
            {
                var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(context.GetRequestHeader(HttpRequestXHeader.Data));
                if (data != null)
                {
                    context.Items.TryAdd(SessionKeys.ClientName.GetDisplayName(), data[SessionKeys.ClientName.GetDisplayName()]);
                    context.Items.TryAdd(SessionKeys.DbName.GetDisplayName(), data[SessionKeys.DbName.GetDisplayName()]);
                    context.Items.TryAdd(SessionKeys.SelectedCultureName.GetDisplayName(), data[SessionKeys.SelectedCultureName.GetDisplayName()]);
                    context.Items.TryAdd(SessionKeys.SelectedOrganizationId.GetDisplayName(), data[SessionKeys.SelectedOrganizationId.GetDisplayName()]);
                    string selectedOrganizationName = Encoding.UTF8.GetString(Convert.FromBase64String(data[SessionKeys.SelectedOrganizationName.GetDisplayName()]));
                    context.Items.TryAdd(SessionKeys.SelectedOrganizationName.GetDisplayName(), selectedOrganizationName);

                    var culture = new CultureInfo(data[SessionKeys.SelectedCultureName.ToString()]);
                    CultureInfo.CurrentCulture = culture;
                    CultureInfo.CurrentUICulture = culture;
                }
            }

            //var dbContext = GeneralContext.ServiceProvider.CreateDBContext<TMSDBContext>();
            //GeneralContext.Container.RegisterInstance<IDbContext>(dbContext);
            //GeneralContext.Container.BuildUp<IDbContext>(dbContext);
            // Call the next delegate/middleware in the pipeline
            await _next(context);
        }
    }
}
