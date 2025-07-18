using System;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using App = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace ID.AcadNet.Services.Test.Extentions
{
    [TestClass()]
    public class UnitTest1
    {
        string fullPath;
        string testFullPath;

        [TestInitialize]
        public void Initialize()
        {
            fullPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\IntelliDesk\\";
            testFullPath = @"C:\Program Files\Autodesk\ApplicationPlugins\IntelliDesk.bundle\Contents\Tests";
        }

        [TestMethod()]
        public void InsertBlockTest()
        {
            Documents.DocumentAction(fullPath + "Test", DocumentOptions.Open);
            var doc = App.DocumentManager.MdiActiveDocument;
            //if (doc != null)
            //    doc.Database.Insert(fullPath,"testblock", new Point3d(100, 100, 0));
            Assert.Fail();
        }
    }
}