using AutoMapper;
using ID.Infrastructure.Core;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace ID.Infrastructure.Controllers
{
    /// <summary> BaseAction for all actions </summary>
    public class GeneralControllerBase : ControllerBase
    {
        #region Public properties

        /// <summary> Current mapper </summary>
        public virtual IMapper Mapper => GeneralContext.GetService<IMapper>();

        /// <summary> Current app config </summary>
        public virtual IAppConfig Appconfig => GeneralContext.GetService<IAppConfig>();

        /// <summary> Current auth options from config</summary>
        public virtual IAuthOptions AuthOptions => GeneralContext.GetService<IAuthOptions>();

        /// <summary> Current httpContext </summary>
        public new HttpContext HttpContext => GeneralContext.GetService<IHttpContextAccessor>()?.HttpContext;

        /// <summary> Current Logged user </summary>
        public IAppUser LoggedUser
        {
            get
            {
                IHttpContextAccessor accessor = (IHttpContextAccessor)GeneralContext.ServiceProvider.GetService(typeof(IHttpContextAccessor));
                var loggedUser = accessor.HttpContext.User.ToAppUser<AppUser>() as AppUser;
                return loggedUser;
            }
        }

        #endregion Public properties

        #region Rest api clients

        private readonly Lazy<GeneralHttpClient> _CalcClient = new Lazy<GeneralHttpClient>(() => GeneralContext.CreateRestClient(ApiServiceNames.CalcApi));
        /// <summary> Current Calculator http rest client for access to CalculatorApi </summary>
        public GeneralHttpClient CalcApi => _CalcClient.Value;

        #endregion Rest api clients
    }
}