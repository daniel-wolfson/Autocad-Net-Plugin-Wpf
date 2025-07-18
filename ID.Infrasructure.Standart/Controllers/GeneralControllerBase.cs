using AutoMapper;
using ID.Infrastructure.Core;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Interfaces;
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
        public IAppUser LoggedUser => GeneralContext.LoggedUser;

        #endregion Public properties

        #region Rest api clients

        private readonly Lazy<GeneralHttpClient> _DBGate = new Lazy<GeneralHttpClient>(() => GeneralContext.CreateRestClient(ApiServiceNames.DalApi));
        /// <summary> Current DBGate http rest client for access to Db </summary>
        public GeneralHttpClient DBGate => _DBGate.Value;

        #endregion Rest api clients
    }
}