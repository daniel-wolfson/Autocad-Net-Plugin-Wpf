using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Interfaces;
using ID.Infrastructure.Resources;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using System;
using System.Globalization;
using System.Reflection;

namespace ID.Infrastructure.Core
{
    public class GeneralLocalizer<TConfig> where TConfig : IAppConfig
    {
        private readonly IStringLocalizer _localizer;
        private readonly ISession _session;
        private readonly IAppConfig _appConfig;
        private readonly string _currentCulture;
        private readonly CultureInfo _cultureHe;
        private readonly CultureInfo _cultureEn;

        public string this[string index]
        {
            get { return _localizer[index]; }
        }

        public Type ResourceType { get; }

        public GeneralLocalizer(IStringLocalizerFactory factory, IHttpContextAccessor httpContextAccessor)
        {
            _session = httpContextAccessor.HttpContext.Session;
            _appConfig = GeneralContext.GetService<TConfig>();
            _currentCulture = _appConfig.DefaultCultureName;
            _cultureHe = new CultureInfo("he-IL");
            _cultureEn = new CultureInfo("en-US");

            if (!_session.Contains(SessionKeys.SelectedCultureName))
                _session.Set(SessionKeys.SelectedCultureName, _currentCulture);

            switch (_appConfig.Domain)
            {
                case "TMS":
                    ResourceType = typeof(GlobalResources);
                    break;
                case "ATD":
                    ResourceType = typeof(GlobalResourcesAtd);
                    break;
                case "EG":
                    ResourceType = typeof(GlobalResourcesEg);
                    break;
                default:
                    ResourceType = typeof(GlobalResources);
                    break;
            }
            string resourceTypeName = ResourceType.Name;

            //Thread.CurrentThread.CurrentCulture = System.Globalization.CultureInfo.CreateSpecificCulture(culture);
            //Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            var assemblyFullName = ResourceType.GetTypeInfo().Assembly.FullName;
            var assemblyName = new AssemblyName(assemblyFullName);
            _localizer = factory.Create(resourceTypeName, assemblyName.Name);
        }

        public LocalizedString GetString(string key)
        {
            LocalizedString result;
            if (_currentCulture.Trim() == "he")
                result = _localizer.WithCulture(_cultureHe)[key];
            else
                result = _localizer.WithCulture(_cultureEn)[key];
            return result;
        }

        public HtmlString GetHtmlString(string key)
        {
            return new HtmlString(GetString(key));
        }
    }
}
