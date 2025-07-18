using System.IO;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.PlottingServices;

namespace Intellidesk.Infrastructure.Gis
{
    public class PlotTo3DDwf
    {
        private string dwgFile, dwfFile, dsdFile, title, outputDir;

        public PlotTo3DDwf()
        {
            outputDir = (string)Application.GetSystemVariable("DWGPREFIX");
            string name = (string)Application.GetSystemVariable("DWGNAME");
            dwgFile = Path.Combine(outputDir, name);
            title = Path.GetFileNameWithoutExtension(name);
            dwfFile = Path.ChangeExtension(dwgFile, "dwf");
            dsdFile = Path.ChangeExtension(dwfFile, ".dsd");
        }

        public PlotTo3DDwf(string outputDir)
            : this()
        {
            this.outputDir = outputDir;
        }

        public void Publish()
        {
            short bgPlot = (short)Application.GetSystemVariable("BACKGROUNDPLOT");
            Application.SetSystemVariable("BACKGROUNDPLOT", 0);
            try
            {
                if (!Directory.Exists(outputDir))
                    Directory.CreateDirectory(outputDir);

                using (DsdData dsd = new DsdData())
                using (DsdEntryCollection dsdEntries = new DsdEntryCollection())
                {
                    // add the Model layout to the entry collection
                    DsdEntry dsdEntry = new DsdEntry();
                    dsdEntry.DwgName = dwgFile;
                    dsdEntry.Layout = "Model";
                    dsdEntry.Title = title;
                    dsdEntry.Nps = "0";
                    dsdEntries.Add(dsdEntry);
                    dsd.SetDsdEntryCollection(dsdEntries);

                    // set DsdData
                    dsd.Dwf3dOptions.PublishWithMaterials = true;
                    dsd.Dwf3dOptions.GroupByXrefHierarchy = true;
                    dsd.SetUnrecognizedData("PwdProtectPublishedDWF", "FALSE");
                    dsd.SetUnrecognizedData("PromptForPwd", "FALSE");
                    dsd.SheetType = SheetType.SingleDwf;
                    dsd.NoOfCopies = 1;
                    dsd.ProjectPath = outputDir;
                    dsd.IsHomogeneous = true;

                    if (File.Exists(dsdFile))
                        File.Delete(dsdFile);

                    // write the DsdData file
                    dsd.WriteDsd(dsdFile);

                    // get the Dsd File contents
                    string str;
                    using (StreamReader sr = new StreamReader(dsdFile, Encoding.Default))
                    {
                        str = sr.ReadToEnd();
                    }
                    // edit the contents
                    str = str.Replace("Has3DDWF=0", "Has3DDWF=1");
                    str = str.Replace("PromptForDwfName=TRUE", "PromptForDwfName=FALSE");
                    // rewrite the Dsd file
                    using (StreamWriter sw = new StreamWriter(dsdFile, false, Encoding.Default))
                    {
                        sw.Write(str);
                    }

                    // import the Dsd file new contents in the DsdData
                    dsd.ReadDsd(dsdFile);

                    File.Delete(dsdFile);

                    PlotConfig pc = PlotConfigManager.SetCurrentConfig("DWF6 ePlot.pc3");
                    Application.Publisher.PublishExecute(dsd, pc);
                }
            }
            finally
            {
                Application.SetSystemVariable("BACKGROUNDPLOT", bgPlot);
            }
        }
    }
}