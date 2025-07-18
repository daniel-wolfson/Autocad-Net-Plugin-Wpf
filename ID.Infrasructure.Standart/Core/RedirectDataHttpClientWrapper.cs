using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.Helpers;
using ID.Infrastructure.Interfaces;
using Newtonsoft.Json;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ID.Infrastructure.Core
{
    public class RedirectDataHttpClientWrapper
    {
        private HttpClient _httpClient;
        private IAppConfig _appConfig;

        public RedirectDataHttpClientWrapper(HttpClient httpClient, IAppConfig config)
        {
            _httpClient = httpClient;
            _appConfig = config;
        }

        public async Task<string> SendAsync(object data, string returnUrl)
        {
            var dataString = JsonConvert.SerializeObject(data);
            //var userByteArray = loggedUser.ToByteArrayUtf8();
            //var encryptData = Convert.ToBase64String(userByteArray);
            IAuthOptions authOptions = GeneralContext.GetService<IAuthOptions>();
            string encryptedData = Util.EncryptText(dataString, authOptions.KEY);

            var httpRequestMessage = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = _httpClient.BaseAddress,
                Content = new StringContent(encryptedData)
            }; //GeneralContext.GetSessionData(_appConfig)
            httpRequestMessage.Headers.Add(HttpRequestXHeader.Data.GetDisplayName(), encryptedData);
            httpRequestMessage.Headers.Add(HttpRequestXHeader.ReturnUrl.GetDisplayName(), returnUrl);

            var response = await _httpClient.SendAsync(httpRequestMessage);

            return response.StatusCode == HttpStatusCode.Redirect
                 ? response.Headers.Location.OriginalString
                 : null;
        }
    }
}
