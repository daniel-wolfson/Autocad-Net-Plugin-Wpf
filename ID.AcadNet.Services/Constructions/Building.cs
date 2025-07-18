using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.AutoCAD.Windows;

namespace Intellidesk.AcadNet.Tools.Constructions
{
    class Building
    {
    }
}


using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.ComponentModel;
using System.IO;

using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Colors;
using Autodesk.AutoCAD.Windows;

using cb.Acd;
using cb.UtilAcd;
using System.Windows.Forms;

namespace Cnt
{
	[DefaultPropertyAttribute("Name")]
	public class Building : BaseDataManager, ICntTabPalette
	{
		private BuildingControl withEventsField_CntBuildingControl = new BuildingControl();
		public BuildingControl CntBuildingControl {
			get { return withEventsField_CntBuildingControl; }
			set {
				if (withEventsField_CntBuildingControl != null) {
					withEventsField_CntBuildingControl.AddFloor_Event -= FloorAdd;
					withEventsField_CntBuildingControl.FloorSelectIndex_Event -= FloorSelect;
					withEventsField_CntBuildingControl.FloorDblClick_Event -= FloorDetail;
				}
				withEventsField_CntBuildingControl = value;
				if (withEventsField_CntBuildingControl != null) {
					withEventsField_CntBuildingControl.AddFloor_Event += FloorAdd;
					withEventsField_CntBuildingControl.FloorSelectIndex_Event += FloorSelect;
					withEventsField_CntBuildingControl.FloorDblClick_Event += FloorDetail;
				}
			}
		}
		private string _Name = "Building";
		[Browsable(false)]
		public bool Complete { get; set; }
		[Browsable(false)]
		public object PalleteControl { get; set; }
		[Browsable(false)]
		public object Parent { get; set; }
		[Browsable(false)]
		public int TabId { get; set; }
		[Browsable(false)]
		public string Header { get; set; }
		[Browsable(false)]
		public bool Current { get; set; }
		[Browsable(false)]
		public bool Visible { get; set; }
		public string Comment { get; set; }
		private int _Number = 0;
		[Browsable(false), CategoryAttribute("Data:"), DisplayName("(id)"), ReadOnly(false)]
		public int Number {
			get { return _Number; }
			set { _Number = value; }
		}
		int ICntTabPalette.UniId {
			get { return Number; }
			set { Number = value; }
		}
		public event PaletteRefreshEventHandler PaletteRefresh;
		public delegate void PaletteRefreshEventHandler();
		public static FloorCollection Floors = new FloorCollection();
		//Public WithEvents EventControl As New FloorControl(Constructions.CntRootNodes.ToArray(), Me)


		#region "Public Const"
		public struct FieldsFormat
		{
			public const string Name = "Floor*";
			public const string elevation = "0.##";
			public static string[] ToList = { Name, elevation };
		}

		public struct FieldsName
		{
			public const string Name = "Building";
			public const string DateCreate = "DateCreate";
			public const string path = "Path";
			public static string[] ToList = { Name, DateCreate, path };
		}

		public struct FieldsPmt
		{
			// Prompt
			public const string  Name = "Building Name";
			public const string  DateCreate = "Create date";
			public const string path = "Path";
			public static string[] ToList = { Name, DateCreate, path };
		}

		public struct FieldsCmt
		{
			//Comment
			public const string Name = "Building Name";
			public const string DateCreate = "Create date";
			public const string path = "Path";
			public static string[] ToList = { Name, DateCreate, path };
		}

		public struct FieldsVal
		{
			//Value Default
			public const string Name = "Building";
			public const string DateCreate = "";
			public const string path = "c:\\";
			public static string[] ToList = { Name, DateCreate, path };
		}

		public struct FieldsDb
		{
			//Value Default
			public const string Name = "Name";
			public const string DateCreate = "DateCreate";
			public const string path = "path";
			public static string[] ToList = { Name, DateCreate, path };
		}

		public struct FieldsType
		{
			public const string Name = "String";
			public const string DateCreate = "Date";
			public const string path = "String";
			public static string[] ToList = { Name, DateCreate, path };
		}

		public struct FieldId
		{
			public const int Name = 0;
			public const int DateCreate = 1;
			public const int path = 2;
			public static int[] ToList = { Name, DateCreate, path };
		}
		#endregion

		#region "PropertyGrid"
		[Category("Data:"), DisplayName(FieldsPmt.Name), DefaultValueAttribute(FieldsVal.Name), DescriptionAttribute(FieldsCmt.Name), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public string Name {
			get { return _Name; }
			set { _Name = value; }
		}

		private System.DateTime _Data = System.DateTime.Now;
		[CategoryAttribute("Data:"), DisplayName(FieldsPmt.DateCreate), DefaultValueAttribute(FieldsVal.DateCreate), DescriptionAttribute(FieldsCmt.DateCreate), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public System.DateTime Data {
			get { return _Data; }
			set { _Data = value; }
		}

	    private string _Path = ""; //base.BasePath;
		[CategoryAttribute("Data:"), DisplayName(FieldsPmt.path), DefaultValueAttribute(FieldsVal.path), DescriptionAttribute(FieldsCmt.path), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public string Path {
			//' name beam
			get { return _Path; }
			set { _Data = value; }
		}

		#endregion

		#region "Constructor"
		public Building(string tFileData) : base(tFileData)
		{
			//-Dim DataProperty As DataManager = New DataManager(BasePath + "FloorDat.xml")
			PaletteMain = new PaletteSetCollection(My.MySettings.Default.ProjectName + " " + My.MySettings.Default.ProjectVersion, this, (PaletteSetStyles.ShowTabForSingle | (PaletteSetStyles.NameEditable | (PaletteSetStyles.ShowPropertiesMenu | (PaletteSetStyles.ShowAutoHideButton | PaletteSetStyles.ShowCloseButton)))), DockSides.Right, new System.Drawing.Size(285, 250), new System.Drawing.Size(285, 858));

			var _with1 = ((BuildingControl)PalleteControl).ListViewFloors;
			_with1.Clear();
			//.Bounds = New System.Drawing.Rectangle(New System.Drawing.Point(10, 10), New System.Drawing.Size(300, 200))
			ImageList imageListSmall = new ImageList();
			imageListSmall.Images.Add(My.Resources.Floor);
			_with1.SmallImageList = imageListSmall;
			_with1.HideSelection = false;
			_with1.FullRowSelect = true;
			_with1.GridLines = true;
			_with1.View = View.Details;
			//.CheckBoxes = True
			_with1.Columns.AddRange(FloorCollection.FloorColumns.ToArray());
		}
		#endregion

		#region "Methods"
		public void Activate(object tObj = null)
		{
			this.Current = true;
			var _with2 = (BuildingControl)PalleteControl;
			_with2.DivComboBox.Items.Clear();
			_with2.ListViewFloors.Items.Clear();
			foreach (Floor Floor in Floors) {
				ListViewItem itm = new ListViewItem(new string[] {
					Floor.Header,
					Floor.Elevation,
					Floor.Constructions.Count
				});
				_with2.ListViewFloors.Items.Add(itm);
				_with2.ListViewFloors.Items(_with2.ListViewFloors.Items.IndexOf(itm)).ImageIndex = 0;
				_with2.DivComboBox.Items.Add(string.Format("{0} Div.{1}", this.Name, Floor.Division.ToString));
			}
			if (_with2.ListViewFloors.Items.Count > 0) {
				_with2.ListViewFloors.Items(Floors(Floors.Count - 1).FloorId).Selected = true;
			}
			if (_with2.ListViewFloors.CanSelect)
				_with2.ListViewFloors.Select();
			_with2.DivComboBox.SelectedIndex = 0;
		}
		void ICntTabPalette.OnActivate(object tObj = null)
		{
			Activate(tObj);
		}
		public void Deactivate()
		{
			this.Current = false;
		}
		void ICntTabPalette.OnDeactivate()
		{
			Deactivate();
		}
		//Deactivate

		public void AddTabCnt(int tCntId, byte tReplaceTabId = null)
		{
			if (tReplaceTabId != null)
				RemoveTab(tReplaceTabId);
			PaletteMain.AddTabCnt(tCntId);
		}

		//ByVal tTabObj As ICntTabPalette
		public void AddTabFloor(int tFloorId, byte tReplaceTabId = null)
		{
			if (tReplaceTabId != null)
				RemoveTab(tReplaceTabId);
			PaletteMain.AddTabFloor(tFloorId);
		}

		public void RemoveTab(int tTabId)
		{
			PaletteMain.RemoveAt(tTabId);
		}

		public void FloorAdd(object tFloor = null)
		{
			if (tFloor == null) {
				Floors.Add(typeof(Floor), this);
				Activate();
			}
		}

		public void FloorSelect(int tItemIndex)
		{
			((BuildingControl)this.PalleteControl).pGrid.SelectedObject = Floors(tItemIndex);
		}

		public void FloorDetail(int tFloorId)
		{
			Cursor.Current = System.Windows.Forms.Cursors.WaitCursor;
			//Dim FloorIdLast As Object = Building.Floors.FindFloor(FindOptions.Visible)
			switch (Building.Floors.CurFLoorId) {
				case tFloorId:
					PaletteMain.Activate(Floors(tFloorId).TabId);
					break;
				case StatusOptions.None:
					this.AddTabFloor(tFloorId, null);
					Building.Floors.CurFLoorId = tFloorId;
					PaletteMain.Activate(Floors(tFloorId).TabId);
					break;
				default:
					Floors(tFloorId).TabId = Floors(Building.Floors.CurFLoorId).TabId;
					this.AddTabFloor(tFloorId, Floors(Building.Floors.CurFLoorId).TabId);
					Building.Floors.CurFLoorId = tFloorId;
					PaletteMain.Activate(Floors(tFloorId).TabId);
					break;
			}
			Cursor.Current = System.Windows.Forms.Cursors.Default;
		}

		//Handles TabBuildingControl.Close_Event
		public void Close(byte tTabId)
		{
			if (tTabId == PaletteMain.Count) {
				PaletteMain.CloseTab(tTabId);
			}
		}

		public void Refresh()
		{
		}

		public void MakeDownTabId()
		{
			if (TabId > 1)
				TabId -= 1;
		}
		#endregion

	}
}

