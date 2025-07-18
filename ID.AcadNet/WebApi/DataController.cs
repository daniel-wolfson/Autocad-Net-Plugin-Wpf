using System;
using System.IO;
using System.Net;
using System.Web.Http;
using Intellidesk.Common;
using Intellidesk.Data.Common.Helpers;
using Intellidesk.Data.Models.Map;
using Intellidesk.Infrastructure.Core;
using Intellidesk.Infrastructure.Extensions;
using Newtonsoft.Json;

namespace Intellidesk.AcadNet.WebApi
{
    public class DataOldController : AcadActionController
    {
        [HttpGet]
        public IHttpActionResult Govim(string id)
        {
            return Load<MapMarkerElement>("GovPoint");
        }

        [HttpGet]
        public IHttpActionResult Kavim(string id)
        {
            return Load<MapLineElement>("KavHafira");
        }

        public IHttpActionResult Load<T>(string path) where T : MapElement
        {
            string data = null;
            string fullPath = Path.GetPathRoot(AppSettings.RootPath).Replace("\\", "") +
                    string.Format(Intellidesk.Resources.Properties.Settings.Default.JsonDataPath, path);
            
            SimpleActionResult serviceResult = JsonActionManager.LoadJsonFileData<T>(fullPath);
            if (serviceResult.StatusCode == HttpStatusCode.Found)
            {
                data = JsonConvert.SerializeObject(serviceResult.ActionResult);
            }

            return Ok().AddHeader(data, "MapIt", new[] { $"Referrer={Url.Content("~/")}", $"Id={Guid.NewGuid()}" });

            //Request.CreateResponse(code, data);
            //return new MapItHeaderActionResult<object>(DateTime.Now, Json(data), Request);
        }
       
    }


}