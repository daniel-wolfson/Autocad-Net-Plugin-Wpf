using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;

namespace Intellidesk.AcadNet.Common.Models
{
    public class XDataRecord
    {
        [DefaultValue(null)]
        public string Name { get; set; }

        [DefaultValue(null)]
        public Handle Handle { get; set; }

        [DefaultValue(null)]
        public Scale3d ScaleFactors { get; set; }

        [DefaultValue(null)]
        public double Rotation { get; set; }

        [DefaultValue(null)]
        public long ColorIndex { get; set; }

        [DefaultValue(null)]
        public int RuleTypeId { get; set; }

        [DefaultValue(null)]
        public Point3d Position { get; set; }

        [DefaultValue(null)]
        public string Key { get; set; }
        
        //public Object Value { get; set; }

        //private static readonly Dictionary<DxfCode, TypedValue> Dic = new Dictionary<DxfCode, TypedValue>();
        //public TypedValue this[XDataRecord code]
        //{
        //    get
        //    {
        //        return Dic[code];
        //    }
        //    set
        //    {
        //        Dic[code] = value;
        //    }
        //}

        public XDataRecord()
        {
        }

        public XDataRecord(ObjectId id)
            : this((BlockReference)id.GetObject(OpenMode.ForRead))
        {
        }

        private XDataRecord(BlockReference br)
        {
            var type = Type.GetType(GetType().Name);
            if (type != null)
            {
                var propertyInfos = type.GetProperties(); //typeof(Type).GetProperties(BindingFlags.Public);
                foreach (var prop in propertyInfos) //.Where(X => !X.Name.Contains("Rule"))
                {
                    if (br.GetType().GetProperty(prop.Name) != null)
                    {
                        var val = Convert.ChangeType(br.GetType().GetProperty(prop.Name).GetValue(br, null), prop.PropertyType);
                        if (prop.CanWrite) { prop.SetValue(this, val, null); }
                    }
                }
            }
        }

        internal TypedValue[] GetTypedValues() //ObjectId? id = null
        {
            var typedValues = new List<TypedValue>();
            //BlockReference br = null;
            //if (id != null) br = (BlockReference)((ObjectId)id).GetObject(OpenMode.ForRead);
            var type = Type.GetType(GetType().Name);
            if (type != null)
            {
                var buff = new StringBuilder("");
                var propertyInfos = type.GetProperties(); //typeof(Type).GetProperties(BindingFlags.Public);
                foreach (var prop in propertyInfos)
                {
                    buff.Clear();
                    //if (id != null)
                    //    buff.Append(prop.Name + ":" + Convert.ToString(br.GetField(prop.Name)));
                    //else
                    var attributes = TypeDescriptor.GetProperties(this)[prop.Name].Attributes;
                    var myAttribute = (DefaultValueAttribute)attributes[typeof(DefaultValueAttribute)];
                    if (prop.GetValue(this, null) != myAttribute.Value)
                    {
                        buff.Append(prop.Name + ":" + Convert.ToString(prop.GetValue(this, null)));
                        typedValues.Add(new TypedValue((int)DxfCode.ExtendedDataAsciiString,
                            buff.ToString().Replace("(", "").Replace(")", "")));
                    }
                }
            }
            return typedValues.ToArray();
        }
    }
}

//switch (prop.Name)
//                    {
//                        case "Name":
//                            buff = prop.Name + ":" + Convert.ToString(br.GetField(prop.Name));
//                            break;
//                        case "Handle":
//                            buff = prop.Name + ":" + Convert.ToString(br.Handle);
//                            break;
//                        case "Scale":
//                            buff = prop.Name + ":" + Convert.ToString(br.ScaleFactors);
//                            break;
//                        case "Rotation":
//                            buff = prop.Name + ":" + Convert.ToString(br.Rotation);
//                            break;
//                        case "ColorIndex":
//                            buff = prop.Name + ":" + Convert.ToString(br.ColorIndex);
//                            break;
//                        case "Position":
//                            buff = prop.Name + ":" + Convert.ToString(br.Position);
//                            break;
//                    }