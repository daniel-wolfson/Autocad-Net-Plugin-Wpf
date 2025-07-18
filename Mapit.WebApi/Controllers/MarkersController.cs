using System.Web.Http;
using System.Web.Http.Cors;

namespace MapIt.WebApi.Controllers
{
    [EnableCors(origins: "http://vmmapinfo/", headers: "*", "*")]
    public class MarkersController : AcadActionController
    {
        // GET api/values
        public IHttpActionResult Get()
        {
            //return LoadMapData<MapMarkerElement>("gov_point");
            return Load("markers_data");
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
