using System.Net.Http;
using MapIt.WebApi.Tests;
using Microsoft.Owin.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MapIt.WebApiTests.Controllers
{
    [TestClass()]
    public class DataControllerTests
    {
        [TestMethod()]
        public async void GovimTest()
        {
            using (var server = TestServer.Create<Startup>())
            {
                using (var client = new HttpClient(server.Handler))
                {
                    var response = await client.GetAsync("http://localhost:43210/MapIt/api/Data/Govim/1");
                    //var result = await response.Content.ReadAsAsync<List<string>>();
                    //Assert.IsTrue(result.Any());
                }
            }

        }
    }
}