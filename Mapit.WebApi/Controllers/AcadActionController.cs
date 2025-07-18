using GeoJSON.Net.Feature;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.General;
using ID.Infrastructure.Interfaces;
using Intellidesk.Data.Common.Helpers;
using Intellidesk.Data.Models.Map;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Remoting.Messaging;
using System.Security.Principal;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;
using System.Web.Http;

namespace MapIt.WebApi.Controllers
{
    //[EnableCors(origins: "http://localhost/MapItApi,http://mapinfo", headers: "*", methods: "*")]
    public class AcadActionController : ApiController
    {
        private static readonly StringBuilder _msg = new StringBuilder();
        public IPluginSettings AppSettings;

        public static StringBuilder ActionMessage
        {
            get { return _msg; }
        }

        public AcadActionController()
        {
            AppSettings = null;
        }
        public AcadActionController(IPluginSettings pluginSettings)
        {
            AppSettings = pluginSettings;
        }

        public static string UserId
        {
            get
            {
                var userId = CallContext.GetData("UserId") as string;
                if (!string.IsNullOrEmpty(userId))
                    return userId;

                WindowsIdentity identity = null;
                if (HttpContext.Current != null)
                {
                    if (null != HttpContext.Current.User && HttpContext.Current.User.Identity is WindowsIdentity)
                        identity = (WindowsIdentity)HttpContext.Current.User.Identity;
                    else if (null != HttpContext.Current.Request.LogonUserIdentity)
                        identity = HttpContext.Current.Request.LogonUserIdentity;
                    else if (null != HttpContext.Current.Request.LogonUserIdentity)
                        identity = WindowsIdentity.GetCurrent();
                }
                else if (null != ServiceSecurityContext.Current)
                    identity = ServiceSecurityContext.Current.WindowsIdentity;

                return null != identity && !string.IsNullOrEmpty(identity.Name)
                    ? identity.Name : Environment.UserName;
            }
        }

        public IHttpActionResult Load(string path)
        {
            string data = "";
            string fullPath = HostingEnvironment.MapPath($@"~/App_Data/{path}.json");

            if (fullPath != null)
                using (StreamReader r = new StreamReader(fullPath))
                {
                    data = r.ReadToEnd();
                }

            return Ok().AddHeader(data, "mapit", new[] { $"Referrer={Url.Content("~/")}", $"Id={Guid.NewGuid()}" });
        }

        public IHttpActionResult LoadMapData<T>(string path) where T : MapElement
        {
            string data = null;

            var fullPath = HostingEnvironment.MapPath($@"~/App_Data/{path}.json");

            SimpleActionResult serviceResult = JsonActionManager.LoadJsonFileData<T>(fullPath);

            if (serviceResult.StatusCode == HttpStatusCode.Found)
            {
                //var serializer = new JavaScriptSerializer();
                //data = serializer.Serialize(serviceResult.ActionResult);
                data = JsonConvert.SerializeObject(serviceResult.ActionResult);
            }

            return Ok().AddHeader(data, "mapit", new[] { $"Referrer={Url.Content("~/")}", $"Id={Guid.NewGuid()}" });

            //Request.CreateResponse(code, data);
            //return new MapItHeaderActionResult<object>(DateTime.Now, Json(data), Request);
        }

        private static async Task<List<Feature>> CancellableCallAsync(string url, CancellationToken cancellationToken)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            using (var response = await client.SendAsync(request, cancellationToken))
            {
                //response.EnsureSuccessStatusCode();
                var content = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var jsonResult = await response.Content.ReadAsStringAsync();
                    var deserializedjsonResult = JObject.Parse(jsonResult);
                    var resultHeroes = deserializedjsonResult["features"];
                    return resultHeroes.Select(x => x.ToObject<Feature>()).ToList();
                }

                return new List<Feature>(); //JsonConvert.DeserializeObject<FeatureCollection>(content);
            }
        }

        // https://weblog.west-wind.com/posts/2013/Dec/13/Accepting-Raw-Request-Body-Content-with-ASPNET-Web-API
        //public IHttpActionResult DoSmoething(string para1, [RawHttpRequestBody] string body = null, string para2) //HttpResponseMessage
        //{
        //}
    }
}
