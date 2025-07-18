using System;
using System.Collections.Generic;
using System.Net.Http;
using Microsoft.Owin.Hosting;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MapIt.WebApi.Tests
{
    [TestClass]
    public class UnitTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            string baseAddress = "http://localhost:9000/";

            // Start OWIN host 
            using (WebApp.Start<Startup>(baseAddress))
            {
                // Create HttpCient and make a request to api/values 
                HttpClient client = new HttpClient();

                var response = client.GetAsync(baseAddress + "api/values").Result;

                Console.WriteLine(response);
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                Console.ReadLine();

            }
        }

       
        [TestMethod]
        public void TestMethod4()
        {
            var csv = new List<string[]>(); // or, List<YourClass>
            var lines = System.IO.File.ReadAllLines(@"C:\Users\user\Desktop\Partner\KavHafira.csv"); //GovPoint.csv
            foreach (string line in lines)
                csv.Add(line.Split(',')); // or, populate YourClass          
            //string json = new System.Web.Script.Serialization.JavaScriptSerializer().Serialize(csv);
        }
    }

}
