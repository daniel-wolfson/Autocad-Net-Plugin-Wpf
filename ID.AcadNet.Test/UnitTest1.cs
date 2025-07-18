using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Net.Http;
using Autodesk.AutoCAD.DatabaseServices;
using Awesomium.Core;
using Intellidesk.AcadNet.Common;
using Intellidesk.AcadNet.Services;
using Intellidesk.Infrastructure.Unity.SelfHostWebApiOwin;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Owin.Hosting;

namespace Intellidesk.AcadNet.Test
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            string baseAddress = "http://localhost:43211/MapIt/";//http://localhost:9000/
            using (WebApp.Start<AcadNetStartUp>("http://localhost:8080/"))
            {
                // Create HttpCient and make a request to api/values 
                HttpClient client = new HttpClient();

                var response = client.GetAsync(baseAddress + "api/Data/Govim/1").Result;

                Console.WriteLine(response);
                Console.WriteLine(response.Content.ReadAsStringAsync().Result);
                Console.ReadLine();
            }
            // Start OWIN host 
            //using (WebApp.Start<Startup>(baseAddress))
        }

        [TestMethod]
        public void TestMethod2()
        {
            using (var webView = WebCore.CreateWebView(800, 600))
            {
                webView.Source = new Uri("http://www.google.com");
                webView.LoadingFrameComplete += (s, e) =>
                {
                    if (!e.IsMainFrame)
                        return;

                    BitmapSurface surface = (BitmapSurface)webView.Surface;
                    surface.SaveToPNG("result.png", true);

                    WebCore.Shutdown();
                };
            }
            //WebCore.Run();
        }

        [TestMethod]
        public void TestMethod3()
        {
            Browser.StartWebCore();
            using (Browser browser = new Browser())
            {
                Stopwatch sw = new Stopwatch();
                sw.Start();
                var url = @"file:///D:\IntelliDesk\IntelliDesk.bundle\Contents\Web\MapIt.html";
                var url1 = "http://google.com";
                Console.WriteLine(browser.GetRenderedHtml(url));
                Console.WriteLine("Time Taken: " + sw.Elapsed.TotalSeconds.ToString("0"));
                Console.ReadLine();

                sw.Reset();
                sw.Start();

                Console.WriteLine(browser.GetRenderedHtml("http://www.amazon.com/dp/B00LF10KNA"));
                Console.WriteLine("Time Taken: " + sw.Elapsed.TotalSeconds.ToString("0"));
                Console.ReadLine();

                sw.Stop();
            }

            Browser.StopWebCore();
        }

        [TestMethod]
        public void TestMethod4()
        {
            var csv = new List<string[]>(); // or, List<YourClass>
            var lines = System.IO.File.ReadAllLines(@"C:\Users\user\Desktop\Partner\KavHafira.csv"); //GovPoint.csv
            foreach (string line in lines)
                csv.Add(line.Split(',')); // or, populate YourClass          
            string json = new
                System.Web.Script.Serialization.JavaScriptSerializer().Serialize(csv);
        }

        //[CommandMethod("zedb")]
        public void ZoomExtentsDb()
        {
            var db = ToolsManager.Db;
            db.ReadDwgFile(@"F:\Dessin1.dwg", System.IO.FileShare.ReadWrite, false, "");
            CommandLine.ZoomExtents(db);
            db.SaveAs(@"F:\Dessin1.dwg", DwgVersion.Current);
        }

    }

}
