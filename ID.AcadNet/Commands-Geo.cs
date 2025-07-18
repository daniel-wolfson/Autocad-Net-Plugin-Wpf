using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using ID.Infrastructure;
using ID.Infrastructure.Commands;
using Intellidesk.AcadNet;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Geo.Extensions;
using Intellidesk.AcadNet.Services.Interfaces;
using Intellidesk.Data.Services;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.ServiceModel;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = Autodesk.AutoCAD.Runtime.Exception;

[assembly: CommandClass(typeof(CommandGeo))]
namespace Intellidesk.AcadNet
{
    public class CommandGeo : CommandLineBase
    {
        private Assembly OnCurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
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

        [CommandMethod(CommandNames.MainGroup, CommandNames.SaveGeoData, CommandFlags.NoHistory)]
        public void SaveGeoDataCommand()
        {
            using (var commandArgs = CommandArgs.Get(CommandNames.SaveGeoData))
            {
                if (commandArgs == null || commandArgs.CancelToken.IsCancellationRequested) return;

                var ids = ((IEnumerable<ObjectId>)commandArgs.CommandParameter).ToList();

                AppDomain.CurrentDomain.AssemblyResolve += AppAssemblyResolver.OnCurrentDomainOnAssemblyResolve;
                ids.XSaveAsGeoJson(commandArgs);
                AppDomain.CurrentDomain.AssemblyResolve -= AppAssemblyResolver.OnCurrentDomainOnAssemblyResolve;

                commandArgs.Clean();
            }
        }

        // Modal Command with localized name
        [CommandMethod("TestGetPolylineCoordinates", CommandFlags.NoHistory)]
        public void GetPolylineCoordinates() // This method can have any name
        {
            Editor ed = acadApp.DocumentManager.MdiActiveDocument.Editor;
            Transaction trans = acadApp.DocumentManager.MdiActiveDocument.Database.TransactionManager
                .StartTransaction();

            using (trans)
            {
                //// Get the Map Object
                //AcMapMap currentMap = AcMapMap.GetCurrentMap();

                //// Prompt user to Select Feature in Map
                //PromptSelectionOptions psop = new PromptSelectionOptions();
                //psop.MessageForAdding = "Select the FDO Feature in Map 3D to read Data : ";
                //psop.SingleOnly = true;
                //PromptSelectionResult psResult = Ed.GetSelection(psop);

                //if (psResult.Status == PromptStatus.OK)
                //{
                //    SelectionSet selSet = psResult.Value;

                //    // Get Map Selectionset from AutoCAD SelectionSet
                //    var mapSelBase = AcMapFeatureEntityService.GetSelection(selSet);
                //    AcMapLayer mapLayer = AcMapFeatureEntityService.GetLayer(psResult.Value[0].ObjectId);

                //    //Get the ID of the selected Parcel
                //    MgFeatureReader ftrRdr = mapSelBase.GetSelectedFeatures(mapLayer, mapLayer.FeatureClassName, false);

                //    while (ftrRdr.ReadNext())
                //    {
                //        MgClassDefinition cd = ftrRdr.GetClassDefinition();

                //        //the geomety property name maybe different for your data source 
                //        MgByteReader byteRdr = ftrRdr.GetGeometry("Geometry");
                //        MgAgfReaderWriter wtr = new MgAgfReaderWriter();

                //        MgGeometry geom = wtr.Read(byteRdr);
                //        if (geom is MgCurveString)
                //        {
                //            var cs = geom as MgCurveString;
                //            Ed.WriteMessage("\n geo is MgCurveString.");

                //            for (int i = 0, segmentCount = cs.Count; i < segmentCount; i++)
                //            {
                //                var seg = cs.GetSegment(i);
                //                if (seg is MgArcSegment)
                //                {
                //                    Ed.WriteMessage("\nthis is an Arc Segment.");
                //                    var arcSeg = seg as MgArcSegment;

                //                    string msg = string.Format("\nstart point: X= {0}, Y={1}",
                //                        arcSeg.StartCoordinate.X,
                //                        arcSeg.StartCoordinate.Y);
                //                    Ed.WriteMessage(msg);

                //                    msg = $"\ncontrol point  X= {arcSeg.ControlCoordinate.X}, Y={arcSeg.ControlCoordinate.Y}";
                //                    Ed.WriteMessage(msg);

                //                    msg = string.Format("\nend point: X= {0}, Y={1}",
                //                        arcSeg.EndCoordinate.X,
                //                        arcSeg.EndCoordinate.Y);
                //                    Ed.WriteMessage(msg);
                //                }
                //                if (seg is MgLinearSegment)
                //                {
                //                    Ed.WriteMessage("\nthis is a linear Segment.");

                //                    var linearSeg = seg as MgLinearSegment;
                //                    var interator = linearSeg.GetCoordinates();
                //                    while (interator.MoveNext())
                //                    {
                //                        var x = interator.GetCurrent().X;
                //                        var y = interator.GetCurrent().Y;

                //                        Ed.WriteMessage(string.Format("\n X = {0}, Y={1} ", x, y));
                //                    }
                //                }

                //            }
                //        }
                //        if (geom is MgLineString)
                //        {
                //            var ls = geom as MgLineString;
                //            var interator = ls.GetCoordinates();
                //            while (interator.MoveNext())
                //            {
                //                var x = interator.GetCurrent().X;
                //                var y = interator.GetCurrent().Y;

                //                Ed.WriteMessage(string.Format(
                //                    "\n X = {0}, Y={1} ", x, y));
                //            }
                //        }

                //    }
            }
            trans.Commit();
        }

        //[CommandMethod("Intellidesk", "IDPOINTONMAPURI", CommandFlags.Session)]
        public void MapInfoGetDataUri(string uri)
        {
            using (var context = new CommandContext("IDPOINTONMAPURI", "GetData"))
            {
                // Create a request using a URL that can receive a post. 
                var request = WebRequest.Create("http://vmmapinfo.partnergsm.co.il/AcadNetGis/PartnerGis/");
                // Set the Method property of the request to POST.
                request.Method = "POST";
                // Create POST data and convert it to a byte array.
                string postData = "This is a test that posts this string to a Web server.";
                byte[] byteArray = System.Text.Encoding.UTF8.GetBytes(postData);
                // Set the ContentType property of the WebRequest.
                request.ContentType = "application/X-www-form-urlencoded";
                // Set the ContentLength property of the WebRequest.
                request.ContentLength = byteArray.Length;
                // Get the request stream.
                System.IO.Stream dataStream = request.GetRequestStream();
                // Write the data to the request stream.
                dataStream.Write(byteArray, 0, byteArray.Length);
                // Close the Stream object.
                dataStream.Close();
                // Get the response.
                WebResponse response = request.GetResponse();
                // Display the status.
                context.CommandLine.WriteMessage(((HttpWebResponse)response).StatusDescription);
                // Get the stream containing content returned by the server.
                dataStream = response.GetResponseStream();
                // Open the stream using a StreamReader for easy access.
                System.IO.StreamReader reader = new System.IO.StreamReader(dataStream);
                // Read the content.
                string responseFromServer = reader.ReadToEnd();

                //var jsonSerializer = new System.Web.Serialization.JavaScriptSerializer.JavaScriptSerializer();
                //var arguments = (IDictionary<string, object>)jsonSerializer.DeserializeObject(e.Argument);
                //var current = jsonSerializer.Deserialize<StateItem>(responseFromServer);

                // Display the content.
                context.CommandLine.WriteMessage(responseFromServer);
                // Clean up the streams.
                reader.Close();
                if (dataStream != null) dataStream.Close();
                response.Close();

                context.Clean();
            }
        }

        private void SendToMapByChannel(double x, double y)
        {
            try
            {
                //using (var cmd = new CommandContext("IDPOINTONMAPCHANNEL", "Sending"))
                //{
                Point3d pnt = Point3d.Origin;
                var ids = Ed.GetSelectImplied();

                if (ids.Count == 0)
                {
                    PromptPointsResult promptPointsResult = Ed.GetPoints("\nEnter the point to send to map", 1);
                    if (promptPointsResult.Status == PromptStatus.OK && promptPointsResult.Value.Any())
                        pnt = promptPointsResult.Value.FirstOrDefault();
                    else
                    {
#if DEBUG
                        //cmd.Cancel("point not selected");
                        //throw new CommandCanceledException("Cancel", new Exception("point not selected"));
#else
                        //throw new OperationCanceledException("Cancel", this.CommandInProcessList[CommandNames.].CancellationToken);
#endif
                    }
                }

                var config = ConfigurationManager<IMapInfoAcmv>.GetConfiguration();
                var group = config.GetSectionGroup();
                var url = group.Client.Endpoints[0].Address;
                var binding = new BasicHttpBinding(); //group.Client.Endpoints[0].Binding;
                var endpoint = new EndpointAddress(url);
                var channelFactory = new ChannelFactory<IMapInfoAcmv>(binding, endpoint);

                IMapInfoAcmv client = null;
                try
                {
                    client = channelFactory.CreateChannel();
                    client.SetViewrXY(pnt.X, pnt.Y);
                    ((ICommunicationObject)client).Close();
                }
                catch (Exception ex)
                {
                    if (client != null)
                        ((ICommunicationObject)client).Abort();
                    //Logger.Error("channelFactory error: ", ex);

#if DEBUG
                    //CommandLine.InProcessList[CommandNames.PointOnMap].IsCanceled = true;
                    throw new CommandCanceledException("Error", ex);
#else
                    throw new OperationCanceledException(ex.ToMessage(CommandNames.PointOnMap),
                        this.CommandInProcessList[CommandNames.PointOnMap].CancellationToken);
#endif
                }
                //}
            }
            catch (OperationCanceledException ex)
            {
                Log.Logger.Error(ex.InnerException.InnerException?.Message ?? ex.Message);
                //this.CommandClose(ex, CommandNames.PointOnMap);
            }
            catch (CommandCanceledException ex)
            {
                Log.Logger.Error(ex.InnerException.InnerException?.Message ?? ex.Message);
                //this.CommandClose(ex, CommandNames.PointOnMap);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.InnerException.InnerException?.Message ?? ex.Message);
                //this.CommandClose("Error", ex.ToNotifyMessage(), CommandNames.PointOnMap);
            }

        }

        public void MiNote(int miWindowID, string message)
        {
            //System.IntPtr MIDispatchPtr = new IntPtr(MIWindowID);
            //DMapInfo MIConnection = (DMapInfo)Marshal.GetObjectForIUnknown(MIDispatchPtr);
            //MIConnection.Do(String.Format("Note \"Note from CSharp: {0}\"", Message));
            //DMBApplications Applications = (DMBApplications)MIConnection.MBApplications;
            //foreach (DMapBasicApplication mbApp in Applications)
            //{
            //    MIConnection.Do(String.Format("Note \"MB App. running in this MapInfo instance: {0}\"", mbApp.Name));
            //}

            //Create a command string that contains the path to our mbx to be run.
            //string appcommand = "Run Application " + "\"" + @"RemoteTest.MBX" + "\"";
            //mapinfoinstance.Do("Open Table \"" + pathName + mapName + ".tab\" Interactive");
            //mapinfoinstance.Do("Open Table " + mapName + " Interactive");
            //name - вызываемый метод, BindingFlags.InvokeMethod - способ связывания, null - связывающий класс, comMapIObject - СОМ объект, Args - аргументы метода
            //comMapIType.InvokeMember("do", BindingFlags.InvokeMethod, null, comMapIObject, new object[] { "Open Table \"" + pathName + mapName + ".tab\"" });

            //Create a instance of Mapinfo
            //string evalResult = mapinfoinstance.Eval("FrontWindow()");
            //var windowId = Int32.Parse(evalResult);
            //var centerX = 5;
            //var centerY = 5;
            //var mapperZoom = 1;

            //var mapinfo = (MapInfoApplication)Marshal.GetActiveObject("Mapinfo.Application");
            //mapinfo.Do("Print \"Hello Partner\" ");
            ////mapinfo.Do("Add Map Auto Layer KAV0912");
            ////mapinfo.Do("Close Table KAV0912 Interactive
            //mapinfo.Do("Open Table \"" + pathName + mapName + ".tab\" Interactive "); //Open Table "C:\Users\Daniel\My Projects\Parner AutoCAD\Y-009-012\GIS\KAV0912.tab" Interactive
            //mapinfo.Do("Map From " + mapName + " ");

            //mapinfo.Do("SearchRect(FrontWindow( ), 0, 0, 2, 2) ");
            //var oType = Type.GetTypeFromProgID("MapInfo.Application");
            //Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(oType.FullName);
            //object o = Activator.CreateInstance(oType);
            //oType.InvokeMember("do", BindingFlags.InvokeMethod, null, o, new object[] { "Set Application Window " + UIManager.PaletteSetCurrent.Handle });
            //oType.InvokeMember("do", BindingFlags.InvokeMethod, null, o, new object[] { "Set Next Document Parent " + UIManager.PaletteSetCurrent.Handle + " Style 1" });
            //oType.InvokeMember("do", BindingFlags.InvokeMethod, null, o, new object[] { "Open Table \"" + pathName + mapName + ".tab\"" });

            //MapInfoApplication _objMI = new MapInfoApplicationClass();
            //_objMI = (MapInfoApplication)Marshal.GetActiveObject("MapInfo.Application");
            //string returnValue = mapinfo.Eval("Set Application Window " + UIManager.PaletteSetCurrent.Handle);
            //returnValue = mapinfo.Eval("Set Next Document Parent " + UIManager.PaletteSetCurrent.Handle + " Style 1");
        }
    }

}

