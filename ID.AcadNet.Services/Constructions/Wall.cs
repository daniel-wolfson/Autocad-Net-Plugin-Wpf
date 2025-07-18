using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;

namespace Intellidesk.AcadNet.Tools.Constructions
{
    public class Wall : Construction, ICntTabPalette, ICntTreeViewData, ICloneable
    {

        private CntDetailControl EventControl = new CntDetailControl();
        #region "Program Property"
        public override int TypeId { get; set; }
        private string _Name = "wall";
        public static new System.Drawing.Image CntImage { get; set; }
        public static new System.Drawing.Image CntImageNew { get; set; }
        public override string Header { get; set; }
        public override int CntId { get; set; }
        public override int FloorId { get; set; }
        public override int CloneId { get; set; }
        public override int TabId { get; set; }
        public override int DataId { get; set; }
        public override int NodeId { get; set; }
        public override bool Complete { get; set; }
        public override object PalleteControl { get; set; }
        public override object Parent { get; set; }
        public override bool Visible { get; set; }
        public override bool Deleted { get; set; }
        public override bool Active { get; set; }
        #endregion

        #region "Public Const"
        public struct FieldsFormat
        {
            public const string nam = "b";
            public const string xb1 = "0.##";
            public const string yb1 = "0.##";
            public const string xb2 = "0.##";
            public const string yb2 = "0.##";
            public const string br = "0.##";
            public const string hr = "0.##";
            public const string bt = "0.##";
            public const string ht = "0.##";
            public const string bb = "0.##";
            public const string hb = "0.##";
            public static string[] ToList = {
				nam,
				xb1,
				yb1,
				xb2,
				yb2,
				br,
				hr,
				bt,
				ht,
				bb,
				hb
			};
        }

        public struct FieldsName
        {
            public const string nam = "b";
            public const string xb1 = "xb1";
            public const string yb1 = "yb1";
            public const string xb2 = "xb2";
            public const string yb2 = "yb2";
            public const string br = "br";
            public const string hr = "hr";
            public const string bt = "bt";
            public const string ht = "ht";
            public const string bb = "bb";
            public const string hb = "hb";
            public static string[] ToList = {
				nam,
				xb1,
				yb1,
				xb2,
				yb2,
				br,
				hr,
				bt,
				ht,
				bb,
				hb
			};
        }

        public struct FieldsPmt
        {
            // Prompt
            public const string nam = "b";
            public const string xb1 = "xb1";
            public const string yb1 = "yb1";
            public const string xb2 = "xb2";
            public const string yb2 = "yb2";
            public const string br = "br";
            public const string hr = "hr";
            public const string bt = "bt";
            public const string ht = "ht";
            public const string bb = "bb";
            public const string hb = "hb";
            public static string[] ToList = {
				nam,
				xb1,
				yb1,
				xb2,
				yb2,
				br,
				hr,
				bt,
				ht,
				bb,
				hb
			};
        }

        public struct FieldsCmt
        {
            //Comment
            public const string nam = "nam";
            public const string xb1 = "xb1";
            public const string yb1 = "yb1";
            public const string xb2 = "xb2";
            public const string yb2 = "yb2";
            public const string br = "br";
            public const string hr = "hr";
            public const string bt = "bt";
            public const string ht = "ht";
            public const string bb = "bb";
            public const string hb = "hb";
            public static string[] ToList = {
				nam,
				xb1,
				yb1,
				xb2,
				yb2,
				br,
				hr,
				bt,
				ht,
				bb,
				hb
			};
        }

        public struct FieldsVal
        {
            //Value Default
            public const string nam = "b";
            public const string xb1 = "0";
            public const string yb1 = "0";
            public const string xb2 = "0";
            public const string yb2 = "0";
            public const string br = "0";
            public const string hr = "0";
            public const string bt = "0";
            public const string ht = "0";
            public const string bb = "0";
            public const string hb = "0";
            public static string[] ToList = {
				nam,
				xb1,
				yb1,
				xb2,
				yb2,
				br,
				hr,
				bt,
				ht,
				bb,
				hb
			};
        }

        public struct FieldsDb
        {
            //Db
            public const string nam = "nam";
            public const string xb1 = "xb1";
            public const string yb1 = "yb1";
            public const string xb2 = "xb2";
            public const string yb2 = "yb2";
            public const string br = "br";
            public const string hr = "hr";
            public const string bt = "bt";
            public const string ht = "ht";
            public const string bb = "bb";
            public const string hb = "hb";
            public static string[] ToList = {
				nam,
				xb1,
				yb1,
				xb2,
				yb2,
				br,
				hr,
				bt,
				ht,
				bb,
				hb
			};
        }

        public struct FieldsType
        {
            public const string nam = "String";
            public const string xb1 = "Double";
            public const string yb1 = "Double";
            public const string xb2 = "Double";
            public const string yb2 = "Double";
            public const string br = "Double";
            public const string hr = "Double";
            public const string bt = "Double";
            public const string ht = "Double";
            public const string bb = "Double";
            public const string hb = "Double";
            public static string[] ToList = {
				nam,
				xb1,
				yb1,
				xb2,
				yb2,
				br,
				hr,
				bt,
				ht,
				bb,
				hb
			};
        }

        public struct FieldId
        {
            public const int nam = 0;
            public const int Number = 1;
            public const int xb1 = 2;
            public const int yb1 = 3;
            public const int xb2 = 4;
            public const int yb2 = 5;
            public const int br = 6;
            public const int hr = 7;
            public const int bt = 8;
            public const int ht = 9;
            public const int bb = 10;
            public const int hb = 11;
            public static int[] ToList = {
				nam,
				xb1,
				yb1,
				xb2,
				yb2,
				br,
				hr,
				bt,
				ht,
				bb,
				hb
			};
        }
        #endregion

        #region "PropertyGrid"
        [CategoryAttribute("Data:"), DisplayName(FieldsPmt.nam), DefaultValueAttribute(FieldsVal.nam), DescriptionAttribute(FieldsCmt.nam), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
        public new string Name
        {
            //' name beam
            get { return _Name; }
            set { _Name = value; }
        }

        [CategoryAttribute("Points:"), DisplayName(FieldsPmt.xb1), DefaultValueAttribute(FieldsVal.xb1), DescriptionAttribute(FieldsCmt.xb1), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
        public double xb1 { get; set; }
        //' coor x point 1

        [CategoryAttribute("Points:"), DisplayName(FieldsPmt.yb1), DefaultValueAttribute(FieldsVal.yb1), DescriptionAttribute(FieldsCmt.yb1), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
        public double yb1 { get; set; }
        //' coor y point 1

        [CategoryAttribute("Points:"), DisplayName(FieldsPmt.xb2), DefaultValueAttribute(FieldsVal.xb2), DescriptionAttribute(FieldsCmt.xb2), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
        public double xb2 { get; set; }
        //' coor x point 2

        [Category("Points:"), DisplayName(FieldsPmt.yb2), DefaultValueAttribute(FieldsVal.yb2), DescriptionAttribute(FieldsCmt.yb2), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
        public double yb2 { get; set; }
        //' coor y point 2

        [Category("cross sect:"), DisplayName(FieldsPmt.br), DefaultValueAttribute(FieldsVal.br), DescriptionAttribute(FieldsCmt.br), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
        public double br { get; set; }
        //' cross sect

        [CategoryAttribute("cross sect:"), DisplayName(FieldsPmt.hr), DefaultValueAttribute(FieldsVal.hr), DescriptionAttribute(FieldsCmt.hr), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
        public double hr { get; set; }
        //' cross sect

        [CategoryAttribute("cross sect:"), DisplayName(FieldsPmt.bt), DefaultValueAttribute(FieldsVal.bt), DescriptionAttribute(FieldsCmt.bt), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
        public double bt { get; set; }
        //' cross sect

        [CategoryAttribute("cross sect:"), DisplayName(FieldsPmt.ht), DefaultValueAttribute(FieldsVal.ht), DescriptionAttribute(FieldsCmt.ht), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
        public double ht { get; set; }
        //' cross sect

        [CategoryAttribute("cross sect:"), DisplayName(FieldsPmt.bb), DefaultValueAttribute(FieldsVal.bb), DescriptionAttribute(FieldsCmt.bb), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
        public double bb { get; set; }
        //' cross sect

        [CategoryAttribute("cross sect:"), DisplayName(FieldsPmt.hb), DefaultValueAttribute(FieldsVal.hb), DescriptionAttribute(FieldsCmt.hb), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
        public double hb { get; set; }
        //' cross sect
        #endregion

        #region "Constructor"
        public Wall()
            : base()
        {
        }
        #endregion

        #region "Methods"
        public override void Activate(object tObj = null)
        {
            //Me.Active = True
            base.Activate(this);
        }
        //Activate
        public override void Deactivate()
        {
            //Me.Active = False
        }
        //Deactivate

        public object Clone()
        {
            Wall temp = new Wall
            {
                CloneId = this.CntId,
                Parent = Parent
            };
            return temp;
        }

        public void DisplayPalette()
        {
        }

        public new void OnPaint(params Point3d[] tParam)
        {
        }
        #endregion

    }
}
