using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using ID.Infrastructure;
using ID.Infrastructure.Commands;
using ID.Infrastructure.Enums;
using ID.Infrastructure.Extensions;
using ID.Infrastructure.General;
using Intellidesk.AcadNet;
using Intellidesk.AcadNet.Common.Core;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.Common.Utils;
using Intellidesk.Data.Common.Helpers;
using Intellidesk.Data.Models.Map;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Windows.Input;

[assembly: CommandClass(typeof(CommandConvert))]

namespace Intellidesk.AcadNet
{
    public class CommandConvert : CommandLineBase
    {
        //[CommandMethod(CommandNames.MainGroup, CommandNames.ConvertFromMapinfo, CommandFlags.Session)]
        public void ConvertFromMapinfoCommand()
        {
            var dataPath = @"D:\IntelliDesk\IntelliDesk.bundle.2018\Contents\WebApi\App_Data\gov_point.json";
            if (!File.Exists(dataPath)) return;

            SimpleActionResult serviceResult = JsonActionManager.LoadJsonFileData<MapMarkerElement>(dataPath);
            if (serviceResult.StatusCode != HttpStatusCode.Found) return;

            List<MapMarkerElement> jsonResult = serviceResult.ActionResult as List<MapMarkerElement>;
            List<BlockReference> objectIds = Db.XReadObjectsDynamic<BlockReference>().ToList();

            if (jsonResult == null) return;

            Notifications.SendNotifyMessageAsync(NotifyStatus.Working);
            Services.CommandLine.SendCancel();
            ProgressMeter pm = new ProgressMeter();
            Thread.Sleep(1000);

            pm.Start("Convert from mapinfo");
            pm.SetLimit(jsonResult.Count);

            using (Transaction tr = Doc.Database.TransactionManager.StartTransaction())
            {
                jsonResult.ForEach(data =>
                {
                    objectIds.ForEach(bref =>
                    {
                        if (bref != null && bref.AttributeCollection.Count > 0)
                        {
                            foreach (ObjectId attrId1 in bref.AttributeCollection)
                            {
                                var attref = (AttributeReference)tr.GetObject(attrId1, OpenMode.ForRead, false);
                                if (attref.Tag == "XXXX" &&
                                    attref.TextString == data.Latitude.GetValueOrDefault().ToString("0.####"))
                                {
                                    foreach (ObjectId attrId2 in bref.AttributeCollection)
                                    {
                                        var attref2 =
                                            (AttributeReference)tr.GetObject(attrId2, OpenMode.ForRead, false);
                                        if (attref2.Tag == "YYYY" &&
                                            attref2.TextString == data.Longitude.GetValueOrDefault().ToString("0.####"))
                                        {
                                            AttributeReference attref3;
                                            foreach (ObjectId attrId3 in bref.AttributeCollection)
                                            {
                                                attref3 = (AttributeReference)tr.GetObject(attrId3, OpenMode.ForRead,
                                                    false);
                                                if (attref3.Tag == "NAME")
                                                {
                                                    attref3.UpgradeOpen();
                                                    attref3.TextString = data.ElementName;
                                                    attref3.DowngradeOpen();
                                                }
                                                else if (attref3.Tag == "FOLDER")
                                                {
                                                    attref3.UpgradeOpen();
                                                    attref3.TextString = data.FolderName;
                                                    attref3.DowngradeOpen();
                                                }
                                                else if (attref3.Tag == "FILE")
                                                {
                                                    attref3.UpgradeOpen();
                                                    attref3.TextString = data.FileName;
                                                    attref3.DowngradeOpen();
                                                }
                                            }
                                            break;
                                        }
                                    }
                                    break;
                                    //    select new { key = attref.Tag, value = attref.TextString }).ToArray();
                                }
                            }
                        }
                    });

                    pm.MeterProgress();
                });
                tr.Commit();
            }

            pm.Stop();
            pm.Dispose();
            Notifications.SendNotifyMessageAsync(NotifyStatus.Ready);
        }

        [CommandMethod(CommandNames.MainGroup, CommandNames.ConvertToMarkers, CommandFlags.Session)]
        public void ConvertToMarkers()
        {
            if (!Utils.IsModelSpace()) return;

            Services.CommandLine.SendCancel();
            Mouse.OverrideCursor = Cursors.Wait;
            Notifications.SendNotifyMessageAsync(NotifyStatus.Working);
            Thread.Sleep(500);

            List<BlockReference> brefs = Db.XReadObjectsDynamic<BlockReference>().Where(x => x.Name.Contains("BLK")).ToList();
            List<ObjectId> explodeObjectList = new List<ObjectId>();

            Dictionary<BlockReference, Dictionary<string, string>> brefItems =
                new Dictionary<BlockReference, Dictionary<string, string>>();

            ProgressMeter pm = new ProgressMeter();
            pm.SetLimit(brefs.Count);
            pm.Start($"Converting {brefs.Count} points to markers...");
            Thread.Sleep(1000);

            using (Doc.LockDocument())
            {
                using (var tr = Db.TransactionManager.StartTransaction())
                {
                    brefs.ForEach(bref =>
                    {
                        if (bref.AttributeCollection.Count > 0)
                            brefItems.Add(bref, bref.XGetAttributes()
                                    .ToDictionary(attr => attr.Tag, attr => attr.TextString));
                    });

                    brefItems.ForEach(brefItem =>
                    {
                        Point3d position = Point3d.Origin;

                        ObjectEventHandler handler =
                            (s, e) =>
                            {
                                if (e.DBObject is DBPoint)
                                    position = ((DBPoint)e.DBObject).Position;
                                explodeObjectList.Add(e.DBObject.ObjectId);
                            };

                        Db.ObjectAppended += handler;
                        brefItem.Key.ExplodeToOwnerSpace();
                        Db.ObjectAppended -= handler;

                        ObjectId brObjectIdNew = Db.InsertBlock("gis_marker", position, new BlockOptions());

                        BlockReference brNew = (BlockReference)tr.GetObject(brObjectIdNew, OpenMode.ForRead);

                        AttributeCollection attrs = brNew.AttributeCollection;

                        foreach (ObjectId attrId in attrs)
                        {
                            AttributeReference newAttr =
                                (AttributeReference)tr.GetObject(attrId, OpenMode.ForRead, false);

                            if (brefItem.Value.ContainsKey(newAttr.Tag))
                            {
                                string curAttrValue = brefItem.Value[newAttr.Tag];
                                newAttr.UpgradeOpen();
                                newAttr.TextString = curAttrValue;
                                newAttr.DowngradeOpen();
                            }
                        }

                        pm.MeterProgress();
                        System.Windows.Forms.Application.DoEvents();
                        Thread.Sleep(20);
                    });
                    tr.Commit();
                }

                if (brefItems.Any())
                {
                    Db.XRemoveObjects(brefs.Select(x => x.ObjectId).ToArray());
                    Db.XRemoveObjects(explodeObjectList.ToArray());
                    Db.SaveAs(Db.OriginalFileName.Replace("." + Files.GetExt(Db.OriginalFileName), ".bak"),
                        DwgVersion.Current);
                    Doc.Editor.Regen();
                    Ed.WriteMessage(CommandNames.UserGroup + ": " + brefItems.Count + " blocks have been converted!");
                }
                else
                {
                    Ed.WriteMessage(CommandNames.UserGroup + ": Block names with start 'BLK' not found!");
                }
            }

            pm.Stop();
            pm.Dispose();

            Notifications.SendNotifyMessageAsync(NotifyStatus.Ready);
            Mouse.OverrideCursor = null;
        }
    }
}

