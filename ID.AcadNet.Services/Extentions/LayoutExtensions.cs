using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using Autodesk.AutoCAD.DatabaseServices;
using App = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Services.Extentions
{
    public static class LayoutExtensions
    {
        public static void SaveDataToXml(this List<Polyline> objetSet, BlockReference br, List<IRule> layerRules, string tName = "")
        {
            var doc = App.DocumentManager.MdiActiveDocument;
            var xmlDoc = new XmlDocument();
            var fileName = Path.GetFileName(doc.Name.Replace(" ", "_"));
            if (fileName != "")
            {
                var rootNode = xmlDoc.CreateElement(fileName);
                xmlDoc.AppendChild(rootNode);

                foreach (var r in layerRules)
                {
                    XmlNode userNode = xmlDoc.CreateElement("LayerType");

                    var attribute = xmlDoc.CreateAttribute("Id"); attribute.Value = Convert.ToString(r.LayerTypeId);
                    if (userNode.Attributes != null) userNode.Attributes.Append(attribute);

                    xmlDoc.CreateAttribute("RuleName"); attribute.Value = r.LayerDestination;
                    if (userNode.Attributes != null) userNode.Attributes.Append(attribute);

                    xmlDoc.CreateAttribute("LayerPattern"); attribute.Value = r.LayerPatternOn.ToString();
                    if (userNode.Attributes != null) userNode.Attributes.Append(attribute);

                    xmlDoc.CreateAttribute(r.ObjectPattern); attribute.Value = r.ObjectPattern;
                    if (userNode.Attributes != null) userNode.Attributes.Append(attribute);

                    rootNode.AppendChild(userNode);

                    var userSubNode = (XmlNode)xmlDoc.CreateElement(br.Name.Replace("*", "_").Replace("$", "S"));
                    userNode.AppendChild(userSubNode);
                    foreach (var obj in objetSet)
                        if (userSubNode.Attributes != null)
                        {
                            xmlDoc.CreateAttribute("Xmin");
                            attribute.Value = Convert.ToString(obj.GeometricExtents.MinPoint.X);
                            userSubNode.Attributes.Append(attribute);

                            xmlDoc.CreateAttribute("Ymin");
                            attribute.Value = Convert.ToString(obj.GeometricExtents.MinPoint.Y);
                            userSubNode.Attributes.Append(attribute);

                            xmlDoc.CreateAttribute("Xmax");
                            attribute.Value = Convert.ToString(obj.GeometricExtents.MaxPoint.X);
                            userSubNode.Attributes.Append(attribute);

                            xmlDoc.CreateAttribute("Ymax");
                            attribute.Value = Convert.ToString(obj.GeometricExtents.MaxPoint.Y);
                            userSubNode.Attributes.Append(attribute);

                            userNode.AppendChild(userSubNode);
                        }
                }
            }
            xmlDoc.Save(doc.Name + ".xml");
        }
    }
}
