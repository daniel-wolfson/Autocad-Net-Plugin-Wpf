using GeoJSON.Net.Feature;
using ID.Infrastructure.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Hosting;
using System.Web.Http;

namespace MapIt.WebApi.Controllers
{
    public class BuildingsController : AcadActionController
    {
        // GET api/values
        public IHttpActionResult Get()
        {
            //return LoadMapData<MapMarkerElement>("gov_point");
            return Load("markers_data");
        }

        public IHttpActionResult Get(double lat1, double lat2, double lng1, double lng2)
        {
            string json_data = "";
            string fullPath = HostingEnvironment.MapPath($@"~/App_Data/buildings_data.json");

            if (string.IsNullOrEmpty(fullPath)) return null;

            using (StreamReader r = new StreamReader(fullPath))
            {
                json_data = r.ReadToEnd();
            }

            FeatureCollection fcResult = new FeatureCollection();
            AppDomain.CurrentDomain.AssemblyResolve += OnCurrentDomainOnAssemblyResolve;

            var obj2 = ReadJSONData(fullPath);
            //var obj2 = await CancellableCallAsync(fullPath, new CancellationToken());
            //JavaScriptSerializer serializer = new JavaScriptSerializer();
            //serializer.MaxJsonLength = 2147483647;
            //var result = serializer.DeserializeObject(json_data);
            //Dictionary<string, object> obj2 = new Dictionary<string, object>();
            //obj2 = (Dictionary<string, object>)(result);

            //FeatureCollection json = Task.Factory.StartNew(() => 
            //    JsonConvert.DeserializeObject<FeatureCollection>(json_data)
            //).Result; //JsonConvert.DeserializeObjectAsync<FeatureCollection>(data);

            AppDomain.CurrentDomain.AssemblyResolve -= OnCurrentDomainOnAssemblyResolve;
            var items = obj2.Last.Last.ToList().Select(x => x.ToObject<Feature>());
            var values = items.Take(100).Select(x => x.Geometry);
            return Ok().AddHeader(JsonConvert.SerializeObject(values), "mapit", new[] { $"Referrer={Url.Content("~/")}", $"Id={Guid.NewGuid()}" });
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody] string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }

        public static Assembly OnCurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            string assemblyFile = args.Name.Contains(',')
                ? args.Name.Substring(0, args.Name.IndexOf(','))
                : args.Name;

            assemblyFile += ".dll";

            string[] LoadAssemblies = { "Newtonsoft.Json.dll" }; // Forbid non handled dll's
            if (!LoadAssemblies.Contains(assemblyFile)) return null;

            string absoluteFolder = new FileInfo(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath).Directory.FullName;
            string targetPath = Path.Combine(absoluteFolder, "Dlls", assemblyFile);

            try
            {
                return Assembly.LoadFile(targetPath);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public JObject ReadJSONData(string jsonFilename)
        {
            try
            {
                JObject jObject;
                // Read JSON directly from a file  
                using (StreamReader file = File.OpenText(jsonFilename))
                using (JsonTextReader reader = new JsonTextReader(file))
                {
                    jObject = (JObject)JToken.ReadFrom(reader);
                }
                return jObject;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error Occurred : " + ex.Message);
                return null;
            }
        }
    }
}
