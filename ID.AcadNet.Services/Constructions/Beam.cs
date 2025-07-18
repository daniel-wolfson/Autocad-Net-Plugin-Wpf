using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Intellidesk.AcadNet.Tools;

namespace Intellidesk.AcadNet.Tools.Constructions
{
    class Beam
    {
    }
}


using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Windows;

using System.Threading;
using System.IO;
using System.ComponentModel;
using System.Math;
using System.Drawing.Design;

using Autodesk.AutoCAD.Windows.Data;
using cb.Acd;
using cb.Cnt;
using cb.UtilAcd;
using System.Windows.Forms.Design;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Reflection;

namespace Cnt
{
	//' BaseManager - parent class for all managers
	public class Beam : Construction, ICntTabPalette, ICntTreeViewData
	{
		private CntDetailControl withEventsField_EventControl = new CntDetailControl();
		private CntDetailControl EventControl {
			get { return withEventsField_EventControl; }
			set {
				if (withEventsField_EventControl != null) {
					withEventsField_EventControl.BtnDraw_Event -= SelectPoints;
					withEventsField_EventControl.Close_Event -= Close;
					withEventsField_EventControl.MenuZoomCnt_Event -= ZoomMe;
					withEventsField_EventControl.MenuRotateCnt_Event -= RotateMe;
					withEventsField_EventControl.MenuInfoCnt_Event -= AboutMe;
				}
				withEventsField_EventControl = value;
				if (withEventsField_EventControl != null) {
					withEventsField_EventControl.BtnDraw_Event += SelectPoints;
					withEventsField_EventControl.Close_Event += Close;
					withEventsField_EventControl.MenuZoomCnt_Event += ZoomMe;
					withEventsField_EventControl.MenuRotateCnt_Event += RotateMe;
					withEventsField_EventControl.MenuInfoCnt_Event += AboutMe;
				}
			}
		}
		//Event CustomEventItem_OnClick_event(ByVal _PointsList As List(Of Point3d))

		private object StoreObj = null;

		public ObjectIdCollection DrawObjectIds = new ObjectIdCollection();
		#region "Program Property"
		public override int TypeId { get; set; }
		private string _Name = "beam";
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
			public const  nam = "b";
			public const  xb1 = "0.##";
			public const  yb1 = "0.##";
			public const  xb2 = "0.##";
			public const  yb2 = "0.##";
			public const  br = "0.##";
			public const  hr = "0.##";
			public const  bt = "0.##";
			public const  ht = "0.##";
			public const  bb = "0.##";
			public const  hb = "0.##";
			public const  BorderType = "";
			public const  place = "";
			public const  fnt = "##";
			public const  axes = "";
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
				hb,
				BorderType,
				place,
				fnt,
				axes
			};
		}

		public struct FieldsName
		{
			public const  nam = "b";
			public const  xb1 = "xb1";
			public const  yb1 = "yb1";
			public const  xb2 = "xb2";
			public const  yb2 = "yb2";
			public const  br = "br";
			public const  hr = "hr";
			public const  bt = "bt";
			public const  ht = "ht";
			public const  bb = "bb";
			public const  hb = "hb";
			public const  plin = "plin";
			public const  place = "place";
			public const  fnt = "fnt";
			public const  axes = "axes";
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
				hb,
				plin,
				place,
				fnt,
				axes
			};
		}

		public struct FieldsPmt
		{
			// Prompt
			public const  nam = "b";
			public const  xb1 = "xb1";
			public const  yb1 = "yb1";
			public const  xb2 = "xb2";
			public const  yb2 = "yb2";
			public const  br = "br";
			public const  hr = "hr";
			public const  bt = "bt";
			public const  ht = "ht";
			public const  bb = "bb";
			public const  hb = "hb";
			public const  plin = "type of drawing";
			public const  place = "placement";
			public const  fnt = "font";
			public const  axes = "axes";
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
				hb,
				plin,
				place,
				fnt,
				axes
			};
		}

		public struct FieldsCmt
		{
			//Comment
			public const  nam = "nam";
			public const  xb1 = "xb1";
			public const  yb1 = "yb1";
			public const  xb2 = "xb2";
			public const  yb2 = "yb2";
			public const  br = "br";
			public const  hr = "hr";
			public const  bt = "bt";
			public const  ht = "ht";
			public const  bb = "bb";
			public const  hb = "hb";
			public const  plin = "type of drawing";
			public const  place = "placement";
			public const  fnt = "font";
			public const  axes = "axes";
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
				hb,
				plin,
				place,
				fnt,
				axes
			};
		}

		public struct FieldsVal
		{
			//Value Default
			public const  nam = "b";
			public const  xb1 = "0";
			public const  yb1 = "0";
			public const  xb2 = "0";
			public const  yb2 = "0";
			public const  br = "0";
			public const  hr = "0";
			public const  bt = "0";
			public const  ht = "0";
			public const  bb = "0";
			public const  hb = "0";
			public const  plin = "pline";
			public const  place = "center";
			public const  fnt = "12";
			public const  axes = "yes";
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
				hb,
				plin,
				place,
				fnt,
				axes
			};
		}

		public struct FieldsDb
		{
			//Db
			public const  nam = "nam";
			public const  xb1 = "xb1";
			public const  yb1 = "yb1";
			public const  xb2 = "xb2";
			public const  yb2 = "yb2";
			public const  br = "br";
			public const  hr = "hr";
			public const  bt = "bt";
			public const  ht = "ht";
			public const  bb = "bb";
			public const  hb = "hb";
			public const  plin = "pline";
			public const  place = "center";
			public const  fnt = "fnt";
			public const  axes = "axes";
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
				hb,
				plin,
				place,
				fnt,
				axes
			};
		}

		public struct FieldsType
		{
			public const  nam = "String";
			public const  xb1 = "Double";
			public const  yb1 = "Double";
			public const  xb2 = "Double";
			public const  yb2 = "Double";
			public const  br = "Double";
			public const  hr = "Double";
			public const  bt = "Double";
			public const  ht = "Double";
			public const  bb = "Double";
			public const  hb = "Double";
			public const  plin = "Byte";
			public const  place = "Byte";
			public const  fnt = "Integer";
			public const  axes = "Boolean";
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
				hb,
				plin,
				place,
				fnt,
				axes
			};
		}

		public struct FieldId
		{
			public const  nam = 0;
			public const  Number = 1;
			public const  xb1 = 2;
			public const  yb1 = 3;
			public const  xb2 = 4;
			public const  yb2 = 5;
			public const  br = 6;
			public const  hr = 7;
			public const  bt = 8;
			public const  ht = 9;
			public const  bb = 10;
			public const  hb = 11;
			public const  plin = 12;
			public const  place = 13;
			public const  fnt = 14;
			public const  axes = 15;
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
				hb,
				plin,
				place,
				fnt,
				axes
			};
		}
		#endregion

		#region "PropertyGrid"
		[CategoryAttribute("Data:"), DisplayName(" Name"), DefaultValueAttribute("b"), DescriptionAttribute("Name of construction, for example: b4 20/40"), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public new string Name {
			get { return _Name; }
			set {
				_Name = value;
				this.Header = value + (this.NodeId + 1).ToString;
				Building.Floors(this.FloorId).Constructions.Item(this.CntId) = this;
				//CType(Me.Parent, Floor)
				Activate(this);
			}
		}

		[CategoryAttribute("Data:"), DisplayName("Height (hr)"), DefaultValueAttribute(FieldsVal.hr), DescriptionAttribute(FieldsCmt.hr), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public double Height { get; set; }
		//' Height

		[CategoryAttribute("Data:"), DisplayName("Width (br)"), DefaultValueAttribute(FieldsVal.br), DescriptionAttribute(FieldsCmt.br), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public double Width { get; set; }
		//' Width

		public override double Lenght { get; set; }

		[CategoryAttribute("Data:"), DisplayName(FieldsPmt.bt), DefaultValueAttribute(FieldsVal.bt), DescriptionAttribute(FieldsCmt.bt), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public double bt { get; set; }
		//' cross sect

		[CategoryAttribute("Data:"), DisplayName(FieldsPmt.ht), DefaultValueAttribute(FieldsVal.ht), DescriptionAttribute(FieldsCmt.ht), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public double ht { get; set; }
		//' cross sect

		[CategoryAttribute("Data:"), DisplayName(FieldsPmt.bb), DefaultValueAttribute(FieldsVal.bb), DescriptionAttribute(FieldsCmt.bb), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public double bb { get; set; }
		//' cross sect

		[CategoryAttribute("Data:"), DisplayName(FieldsPmt.hb), DefaultValueAttribute(FieldsVal.hb), DescriptionAttribute(FieldsCmt.hb), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public double hb { get; set; }
		//' cross sect

		public override System.Drawing.Font Font { get; set; }
		//' fnt
		public override System.Drawing.Color FontColor { get; set; }
		//BorderColor

		private int _Border = BorderOptions.Pline;
		public override int Border {
			get { return _Border; }
			set { _Border = value; }
		}
		public override System.Drawing.Color BorderColor { get; set; }

		private int _Placement = BeamPlaceOptions.Center;
		[TypeConverter(typeof(Converters.PlacementStyleInOutConverter))]
		public override int Placement {
			get { return _Placement; }
			set { _Placement = value; }
		}

		[DefaultValueAttribute(FieldsVal.axes), DescriptionAttribute(FieldsCmt.axes)]
		public override bool Axes { get; set; }
		//axes

		public double RotationRadian = 0;
		[DisplayName("Rotation"), DefaultValueAttribute("0"), DescriptionAttribute("Rotation from base start")]
		public override int RotationDegree {
			//Rotation
			get { return GeomManagement.RadianToDegree(RotationRadian); }
			set { RotationRadian = GeomManagement.DegreeToRadian(value); }
		}

		[DisplayName("First point"), DefaultValueAttribute("0,0"), DescriptionAttribute("Point start")]
		public override Drawing.Point CntStartPoint { get; set; }
		//point 1
		[DisplayName("End point"), DefaultValueAttribute("0,0"), DescriptionAttribute("Point end")]
		public override Drawing.Point CntEndPoint { get; set; }
		//point 2

		private List<Drawing.Point> _PntList = new List<Drawing.Point>();
		public override List<Drawing.Point> SelectedPoints {
			get { return _PntList; }
			set {
				_PntList.AddRange(value);
				if (value != null) {
					CntStartPoint = value(0);
					CntEndPoint = value(1);
					//xb1 = PntList(0).X : yb1 = PntList(0).Y :xb2 = PntList(1).X : yb2 = PntList(1).Y
					this.Refresh();
					//PaletteMain.Activate(Me.TabId)
					//If tPaint Then OnPaint(PntList) 'EventControl.pGrid.Refresh()
				}
			}
		}

		public override string Comment { get; set; }

		//Private m_foo As New Points
		//<Editor(GetType(UICustomEventEditor), GetType(UITypeEditor)), Description("Selected Points Colllection")> _
		//<Browsable(True), CategoryAttribute("Points:"), DisplayName("Points Select")> _
		//Public ReadOnly Property SelectedPoints() As Points
		//    Get
		//        Return m_foo
		//    End Get
		//End Property
		#endregion

		#region "Constructor"
		public Beam() : base()
		{
			//AddHandler Me.PalleteControl.pgrid.selectpoints.onclick, AddressOf CustomEventItem_OnClick
		}
		#endregion

		#region "Methods"
		public override void Activate(object tObj = null)
		{
			base.Activate(this);
		}
		//Activate
		public override void Deactivate()
		{
		}
		//Deactivate

		private bool IsUpdate()
		{
			this.StoreObj = Building.Floors(this.FloorId).Constructions(CntId);
			object c = ((CntDetailControl)this.PalleteControl).pGrid.SelectedObject;
			System.Reflection.PropertyInfo[] prs = null;
			System.Reflection.PropertyInfo[] prsStore = null;
			string propName = null;
			string propName1 = null;
			dynamic propValue = null;
			dynamic propValue1 = null;

			PropertyInfo[] propsInfo = c.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
			PropertyInfo[] popsInfstore = StoreObj.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public);
			prs = c.GetType().GetProperties();
			prsStore = StoreObj.GetType().GetProperties();
			for (j = 0; j <= prs.Length - 1; j++) {
				//Dim att = prop.GetCustomAttributes(True)
				//Debug.WriteLine(prop.GetValue(c, Nothing))
				if (prs(j).CanRead & prs(j).CanWrite) {
					propName = prs(j).Name;
					propName1 = prsStore(j).Name;

					propValue = prs(j).GetValue(c, null);
					propValue1 = prsStore(j).GetValue(c, null);

					if (propName == "br") {
						if (propValue.Equals(propValue1)) {
							Debug.WriteLine("Ok");
							this.StoreObj = this.MemberwiseClone();
						} else {
							Debug.WriteLine("no");
						}
					}

				}
			}

			//Return New PropertyDescriptorCollection(DirectCast(list.ToArray(GetType(PropertyDescriptor)), PropertyDescriptor()))
			return false;
		}

		public object Clone()
		{
			Beam temp = new Beam {
				CloneId = this.CntId,
				Parent = Parent
			};
			//With temp
			//    For Each p As System.Reflection.PropertyInfo In Me.GetType().GetProperties()
			//        If p.CanRead Then
			//            temp.MemberwiseClone.SetPropertyValue(p.Name, p.GetValue(Me, Nothing))
			//        End If
			//    Next
			//End With
			return temp;
		}

		public void DisplayPalette()
		{
		}

		public new void SelectPoints(int tCntId, bool tPaint = false)
		{
			//Building.PaletteMain
			foreach (ICntTabPalette p in PaletteMain) {
				if (p.Current) {
					((ICntTabPalette)PaletteMain(p.TabId)).PalleteControl.pgrid.@select();
				}
			}
			List<Point3d> PntList = DrawManager.SelectPoints(2, SelectPointsOptions.Point3d);
			if (PntList != null) {
				this.CntStartPoint = new Drawing.Point(Convert.ToInt32(PntList(0).X), Convert.ToInt32(PntList(0).Y));
				this.CntEndPoint = new Drawing.Point(Convert.ToInt32(PntList(1).X), Convert.ToInt32(PntList(1).Y));
				this.Refresh();
				if (tPaint)
					OnPaint(1, PntList);
				//PaletteMain.Activate(Me.TabId)
			}
		}

		//ParamArray tParam() As Object
		public override void OnPaint(byte f = 1, List<Point3d> tPntList = null)
		{
			DBObject Obj = null;
			Point3d Pnt = null;
			if (this.Border == BorderOptions.Pline) {
				Obj = DrawManager.CntPline(new Point2d(this.CntStartPoint.X, this.CntStartPoint.Y), this.Lenght, this.Width, this.BorderColor, 0, 1, this.RotationRadian, , this.Placement);
				DrawObjectIds.Add(Obj.ObjectId);
				CmdManager.SetXData(Obj.ObjectId, "CntId=" + CntId.ToString.Trim, "TypeId=" + TypeId.ToString, "FloorId=" + FloorId.ToString, "Placement=" + Placement.ToString, "Rotate=" + RotationRadian.ToString);
				Pnt = ((Polyline)Obj).StartPoint;
			} else {
				Obj = DrawManager.CntLine(new Point2d(this.CntStartPoint.X, this.CntStartPoint.Y), this.Lenght, this.Width, 0, this.BorderColor, this.RotationRadian, , this.Placement, false);
				DrawObjectIds.Add(Obj.ObjectId);
				CmdManager.SetXData(Obj.ObjectId, "CntId=" + CntId.ToString.Trim, "TypeId=" + TypeId.ToString, "FloorId=" + FloorId.ToString, "Placement=" + Placement.ToString, "Rotate=" + RotationRadian.ToString);
				Pnt = ((Line)Obj).StartPoint;
			}
			//If f = 0 Or f = 2 Then
			//    'Obj = DrawManager.sol(px, py, 0)
			//    CmdManager.SetXData(Obj.ObjectId, "CntId=" + CntId.ToString.Trim, "TypeId=" + TypeId.ToString, "FloorId=" + FloorId.ToString, "Placement=" + Placement.ToString, "Rotate=" + RotationDegree.ToString)
			//End If
			Obj = DrawManager.txt(Pnt.X + this.Lenght / 2, Pnt.Y + this.Width / 2, this.Header + " " + this.Width.ToString + "/" + this.Height.ToString, 2, FontColor, RotationRadian, this.CntStartPoint.X, this.CntStartPoint.Y + this.Width / 2, Font.Height);
			DrawObjectIds.Add(Obj.ObjectId);
			CmdManager.SetXData(Obj.ObjectId, "CntId=" + CntId.ToString.Trim, "TypeId=" + TypeId.ToString, "FloorId=" + FloorId.ToString, "Placement=" + Placement.ToString, "Rotate=" + RotationRadian.ToString);
			if (this.Axes) {
				Obj = DrawManager.CntLine(new Point2d(this.CntStartPoint.X, this.CntStartPoint.Y), this.Width, this.Lenght, 1, this.BorderColor, this.RotationRadian, , true);
				CmdManager.SetXData(Obj.ObjectId, "CntId=" + CntId.ToString.Trim, "TypeId=" + TypeId.ToString, "FloorId=" + FloorId.ToString, "Placement=" + Placement.ToString, "Rotate=" + RotationRadian.ToString);
			}
		}

		public void Close()
		{
			PaletteMain.RemoveAt(this.TabId);
			//  RemoveTab
			this.Visible = false;

			//If Building.Floors.Current = Me.FloorId Then
			//    PaletteMain.Activate(0)
			//Else
			//    Dim FloorLast As Floor = Building.Floors.FindFloor(FindOptions.Visible)
			//    If FloorLast IsNot Nothing Then
			//        Building.Floors.Current = FloorLast.FloorId
			//        PaletteMain.Activate(FloorLast.TabId)
			//    Else
			//        Building.Floors.Current = FloorOptions.None
			//    End If
			//End If
			//Me.Dispose()
		}
		//Close

		private object CustomEventItem_OnClick(object sender, EventArgs e)
		{
			Interaction.MsgBox("You clicked on property '" + sender.CustomProperty.Name + "'", MsgBoxStyle.Information, "Custom Events as UITypeEditor");
			return "Click me again";
		}

		private void ZoomMe()
		{
			CmdManager.Zoom(CmdManager.GetObjXData(this.CntId));
		}

		private void RotateMe(SelectOptions tOption)
		{
			CmdManager.RotateEntity(this.CntId, tOption);
		}

		private void AboutMe()
		{
			CmdManager.GetObjXData(this.CntId, SelectOptions.ToDisplay);
		}

		public override void Refresh()
		{
			DrawObjectIds.Clear();
			foreach (ObjectId objId in CmdManager.GetObjXData(this.CntId)) {
				this.DrawObjectIds.Add(objId);
			}

			Line tmpline = new Line(new Point3d(CntStartPoint.X, CntStartPoint.Y, 0), new Point3d(CntEndPoint.X, CntEndPoint.Y, 0));
			this.Lenght = Strings.Format(Math.Abs(tmpline.Length), "#.000");
			//'Me.Lenght = Math.Round(tmpline.Length, 2, MidpointRounding.AwayFromZero)
			RotationRadian = tmpline.Angle;

			Building.Floors(this.FloorId).Constructions(this.CntId) = this;
			((CntDetailControl)this.PalleteControl).pGrid.SelectedObject = this;
			((CntDetailControl)this.PalleteControl).pGrid.Refresh();
		}
		#endregion

	}
	//<Browsable(True), CategoryAttribute("Points select:"), [ReadOnly](True)> _
}

