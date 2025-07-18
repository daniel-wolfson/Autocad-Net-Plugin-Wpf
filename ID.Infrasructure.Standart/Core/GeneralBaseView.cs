using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Threading.Tasks;

namespace ID.Infrastructure.Core
{
    public abstract class GeneralBaseView<TModel, TConfig> : RazorPage<TModel>
    {
        public bool IsAuthenticated()
        {
            return Context.User.Identity.IsAuthenticated;
        }

        public override Task ExecuteAsync()
        {
            throw new NotImplementedException();
        }

        private HttpContext _httpContext;
        public HttpContext HttpContext
        {
            get
            {
                if (_httpContext?.Session == null)
                {
                    IHttpContextAccessor httpContextAccessor = GeneralContext.GetService<IHttpContextAccessor>();
                    _httpContext = httpContextAccessor.HttpContext;
                }

                TConfig appConfig = GeneralContext.GetService<TConfig>();
                ((IAppConfig)appConfig).DefaultCultureName = _httpContext.Session.Get(SessionKeys.SelectedCultureName).ToString();

                if (ViewData.ContainsKey(SessionKeys._rtl.ToString()))
                {
                    ViewData[SessionKeys._rtl.ToString()] = Session.Get(SessionKeys._rtl);
                    ViewData[SessionKeys._dir.ToString()] = Session.Get(SessionKeys._dir);
                    ViewData[SessionKeys._float.ToString()] = Session.Get(SessionKeys._float);
                    ViewData[SessionKeys._notfloat.ToString()] = Session.Get(SessionKeys._notfloat);
                }

                return _httpContext;
            }
        }

        public ISession Session { get { return HttpContext.Session; } }

        //private readonly Lazy<GeneralLocalizer> _localizer = new Lazy<GeneralLocalizer>(() =>
        //{
        //    var localizer = GeneralContext.GetService<GeneralLocalizer>();
        //    return localizer;
        //});
        //public GeneralLocalizer Localizer => _localizer.Value;

        private readonly Lazy<TConfig> _appConfig = new Lazy<TConfig>(() =>
        {
            TConfig appConfig = GeneralContext.GetService<TConfig>();
            return appConfig;
        });
        public TConfig AppConfig => _appConfig.Value;

        private AppUser _loggedUser;
        public AppUser LoggedUser
        {
            get
            {
                if (_loggedUser == null)
                {
                    var loggedUser = Session.Get<AppUser>(SessionKeys.AppUser);
                    if (loggedUser == null)
                    {
                        //Log.Logger.ErrorCall("loggedUser undefined");
                        //HttpContext.Response.Redirect("/Home/Login", true);
                    }
                    _loggedUser = loggedUser;
                }
                return _loggedUser;
            }
        }

        public string Rtl { get { return Session.Get<string>(SessionKeys._rtl); } }

        public string Dir { get { return Session.Get<string>(SessionKeys._dir); } }
    }
}
