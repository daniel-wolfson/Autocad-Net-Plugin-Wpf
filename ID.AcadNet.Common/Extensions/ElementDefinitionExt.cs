using Autodesk.AutoCAD.DatabaseServices;
using ID.Infrastructure;
using ID.Infrastructure.Interfaces;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.Data.Models.Entities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace Intellidesk.AcadNet.Common.Extensions
{
    public static class ElementDefinitionExt
    {
        public static TypedValue[] GetDataTypeValues(this IPaletteElement element)
        {
            IPluginSettings pluginSettings = Plugin.GetService<IPluginSettings>();
            TypedValue[] typedValues = {
                new TypedValue((int) DxfCodeExt.AppName, pluginSettings.Name),
                new TypedValue((int) DxfCode.ExtendedDataAsciiString, "TypeName_" + element.GetType().Name),
                new TypedValue((int) DxfCode.ExtendedDataAsciiString, "TypeFullName_" + element.GetType().FullName),
                new TypedValue((int) DxfCode.ExtendedDataAsciiString, "TypeCode_" + element.GetType().Name),
                new TypedValue((int) DxfCode.ExtendedDataAsciiString, "Object_" + JsonConvert.SerializeObject(element)),
                new TypedValue((int) DxfCode.ExtendedDataAsciiString, "LayerName_" + element.LayerName),
                new TypedValue((int) DxfCode.ExtendedDataAsciiString, "Handle_" + element.Handle),
                new TypedValue((int) DxfCode.ExtendedDataAsciiString, "OwnerHandle_" + element.OwnerHandle),
                new TypedValue((int) DxfCode.ExtendedDataAsciiString, "OwnerType_" + element.OwnerFullType),
                new TypedValue((int)DxfCode.ExtendedDataAsciiString, "Items_" + (element.Items.Length > 0 ? string.Join(",", element.Items) : " ")) //1004

                //new TypedValue((int)DxfCode.ExtendedDataControlString, "{"),        //1002
                //    new TypedValue((int)DxfCode.ExtendedDataAsciiString, "dfgdfgdfgdfg"),          //1000
                //    new TypedValue((int)DxfCode.ExtendedDataReal, 12345.6789),          //1040
                //    new TypedValue((int)DxfCode.ExtendedDataDist, 25.25),               //1041
                //    new TypedValue((int)DxfCode.ExtendedDataScale, 0.2),                //1042
                //new TypedValue((int)DxfCode.ExtendedDataControlString, "}")        //1002

                //new TypedValue((int) DxfCode.Comment, "")
                //new TypedValue((int) DxfCodeExt.DataType, element.GetType().Name),
                //new TypedValue((int) DxfCodeExt.Items, element.Items != null && element.Items.Length > 0 ? string.Join(",", element.Items) : ""), //element.Name + element.TypeName
                //new TypedValue((int)DxfCode.Start, pluginSettings.Name), //KAppCode = 101 ResultBuffer rb = acEnt.GetXDataForApplication(appName);
                //new TypedValue((int) DxfCode.ArbitraryHandle, ownerHandle ?? ""),
                //new TypedValue((int) DxfCode.XTextString, element.Handles != null && element.Handles.Length > 0 ? string.Join(",", element.Handles) : ""),
                //new TypedValue((int) DxfCode.Color, element.ColorIndex),
            };
            return typedValues;
        }


        //var serObj = cable.SaveToResBuf(PluginSettings.Name);
        //TypedValue typedValue = new TypedValue((int) DxfCode.ExtendedDataBinaryChunk, serObj);
        //new TypedValue((int)DxfCode.Operator, "<=,<=,*"),
        //new TypedValue((int)DxfCode.XCoordinate, pnt),
        //new TypedValue((int)DxfCode.Operator, ">=,>=,*"),
        //new TypedValue((int)DxfCode.XCoordinate, pnt));

        public static Dictionary<DxfCode, object> GetDataTypeFilter(this Dictionary<DxfCode, object> dic)
        {
            return dic.Where(x => x.Key != DxfCode.ExtendedDataAsciiString).ToDictionary(x => x.Key, y => y.Value);
        }

        public static T Clone<T>(this T obj)
        {
            var inst = obj.GetType().GetMethod("MemberwiseClone", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            return (T)inst?.Invoke(obj, null);
        }
    }
}
