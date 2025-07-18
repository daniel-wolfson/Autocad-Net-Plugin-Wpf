using System.Web.Http;

namespace MapIt.WebApi.Controllers
{
    public class LinesController : AcadActionController
    {
        // GET api/values
        public IHttpActionResult Get()
        {
            return Load("lines_data");
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
