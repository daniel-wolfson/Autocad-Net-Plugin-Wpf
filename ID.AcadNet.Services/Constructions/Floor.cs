using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intellidesk.AcadNet.Tools.Constructions
{
    class Floor
    {
    }
}

namespace Cnt
{
	[DefaultProperty("Name")]
	public class Floor : BaseDataManager, IDisposable, ICntTabPalette, ICloneable
	{
		public CntCollection Constructions = new CntCollection();
		public static List<System.Windows.Forms.ColumnHeader> FloorColumns = new List<System.Windows.Forms.ColumnHeader>();
		private FloorControl withEventsField_EventControl;
		public FloorControl EventControl {
			get { return withEventsField_EventControl; }
			set {
				if (withEventsField_EventControl != null) {
					withEventsField_EventControl.AddConstruction_Event -= AddConstruction;
					withEventsField_EventControl.DelConstruction_Event -= DelConstruction;
					withEventsField_EventControl.DetailConstruction_Event -= DetailConstruction;
					withEventsField_EventControl.ZoomConstruction_Event -= ZoomConstruction;
					withEventsField_EventControl.MenuRotate_Event -= RotateCnt;
					withEventsField_EventControl.MenuCntInfo_Event -= AboutConstruction;
					withEventsField_EventControl.Close_Event -= Close;
					withEventsField_EventControl.NodeMouseEnter -= NodeMouseEnter;
				}
				withEventsField_EventControl = value;
				if (withEventsField_EventControl != null) {
					withEventsField_EventControl.AddConstruction_Event += AddConstruction;
					withEventsField_EventControl.DelConstruction_Event += DelConstruction;
					withEventsField_EventControl.DetailConstruction_Event += DetailConstruction;
					withEventsField_EventControl.ZoomConstruction_Event += ZoomConstruction;
					withEventsField_EventControl.MenuRotate_Event += RotateCnt;
					withEventsField_EventControl.MenuCntInfo_Event += AboutConstruction;
					withEventsField_EventControl.Close_Event += Close;
					withEventsField_EventControl.NodeMouseEnter += NodeMouseEnter;
				}
			}
			//(Constructions.CntRootNodes.ToArray(), Me)
		}
		private LayerManagment LayerSet;
		private TreeNodeCollection TreeConstructions;

		private string _Name = "Floor";
		#region "Public Property"
			//'New FloorControl(Constructions.CntRootNodes.ToArray(), Me)
		private FloorControl _PalleteControl = null;
		[Browsable(false)]
		public object PalleteControl {
			get { return _PalleteControl; }
			set { _PalleteControl = (FloorControl)value; }
		}
		[Browsable(false)]
		public bool Complete { get; set; }
		bool ICntTabPalette.Visible {
			get { return Complete; }
			set { Complete = value; }
		}
		[Browsable(false)]
		public object Parent { get; set; }
		[Browsable(false)]
		public int TabId { get; set; }
		[Browsable(false)]
		public bool Visible { get; set; }
		bool ICntTabPalette.Complete {
			get { return Visible; }
			set { Visible = value; }
		}
		[Browsable(false)]
		public bool Current { get; set; }
		[Browsable(false)]
		public int CloneId { get; set; }
		[Browsable(false)]
		public string Header { get; set; }
		public string Comment { get; set; }
		[Browsable(false), CategoryAttribute("Data:"), DisplayName("(id)"), ReadOnly(true)]
		public int Number {
			//' number beam in collect Bems
			get { return this.FloorId; }
		}
		#endregion

		#region "Public Const"
		public struct FieldsFormat
		{
			public const string Name = "Floor*";
			public const string Id = "0";
			public const string elevation = "0.##";
			public const string division = "0.##";
			public static string[] ToList = { Name, Id, elevation, division };
		}

		public struct FieldsName
		{
			public const string Name = "Name";
			public const string Id = "Number";
			public const string elevation = "Elevation";
			public const string division = "Division";
			public static string[] ToList = { Name, Id, elevation, division };
		}

		public struct FieldsPmt
		{
			// Prompt
			public const string Name = "Name";
			public const string Id = "Number";
			public const string  elevation = "Elevation";
			public const string division = "Division";
			public static string[] ToList = { Name, Id, elevation, division };
		}

		public struct FieldsCmt
		{
			//Comment
			public const string Name = "User name of Floor";
			public const string Id = "Number of Floor";
			public const string elevation = "Elevation";
			public const string division = "Division";
			public static string[] ToList = { Name,  Id, elevation, division };
		}

		public struct FieldsVal
		{
			//Value Default
			public const string Name = "Floor";
			public const string Id = "0";
			public const string elevation = "0";
			public const string division = "0";
			public static string[] ToList = { Name, Id, elevation, division };
		}

		public struct FieldsDb
		{
			//Value Default
			public const string Name = "Floors";
			public const string Id = "Floors";
			public const string elevation = "Floors";
			public const string division = "Division";
			public static string[] ToList = { Name, Id, elevation, division };
		}

		public struct FieldsType
		{
			public const string Name = "String";
			public const string Id = "Integer";
			public const string elevation = "Double";
			public const string division = "Integer";
			public static string[] ToList = { Name, Id, elevation, division };
		}

		public struct FieldId
		{
			public const int Name = 0;
            public const int Id = 1;
            public const int elevation = 2;
            public const int division = 3;
            public static int[] ToList = { Name, Id, elevation, division };
		}
		#endregion

		#region "PropertyGrid"
		[CategoryAttribute("Data:"), DisplayName(FieldsPmt.Name), DefaultValueAttribute(FieldsVal.Name), DescriptionAttribute(FieldsCmt.Name), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public string Name {
			get { return _Name; }
			set { _Name = value; }
		}

		private int _FloorId = 0;
		[CategoryAttribute("Data:"), DisplayName(FieldsPmt.Id), DefaultValueAttribute(FieldsVal.Id), DescriptionAttribute(FieldsCmt.Id), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public int FloorId {
			get { return _FloorId; }
			set { _FloorId = value; }
		}
		int ICntTabPalette.UniId {
			get { return FloorId; }
			set { FloorId = value; }
		}

		private string _Elevation = "+1";
		[CategoryAttribute("Data:"), DisplayName(FieldsPmt.elevation), DefaultValueAttribute(FieldsVal.elevation), DescriptionAttribute(FieldsCmt.elevation), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public string Elevation {
			get { return _Elevation; }
			set { _Elevation = value; }
		}

		private int _Div = 0;
		[CategoryAttribute("Data:"), DisplayName(FieldsPmt.division), DefaultValueAttribute(FieldsVal.division), DescriptionAttribute(FieldsCmt.division), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public int Division {
			get { return _Div; }
			set { _Div = value; }
		}

		#endregion

		#region "Constructor"
		public Floor(string tFileData) : base(tFileData)
		{
			//AddHandler EventControl.AddConstruction_Event, AddressOf AddConstruction
			//AddHandler PaletteSetCollection.PaletteSetMain.PaletteActivated, AddressOf PalleteActivate
			///'AddHandler EventControl.CntTreeView.DrawNode, AddressOf OnDrawNode
			///'EventControl.CntTreeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawAll
			try {
				//'EditManager.OpenDwg(ProjectManager.BasePath, "1234")
				if (!LayerManager.LayerNames.SequenceEqual({
					"cbCXEMA",
					"cbDIM"
				})) {
					LayerManager.LayerNames = new string[] {
						"cbCXEMA",
						"cbDIM"
					};
					LayerManager.LayerColors = new Color[] {
						LayerManagment.Colors.White,
						LayerManagment.Colors.Yellow
					};
					EditManager.WorkSpaceInit();
					LayerManager.AutoComplete(, , true);
				}
				//'Dim DataProperty As DataManager = New DataManager(BasePath + "FloorDat.xml")
			} catch (System.Exception ex) {
				Ed.WriteMessage("Floor.New" + System.Reflection.MethodBase.GetCurrentMethod.Name(), ex.Message);
			}
			//CType(Me.PalleteControl, FloorControl).CntTreeView.ImageList = CntCollection.CntImgList
		}
		#endregion

		#region "Methods for Constructions"
		//' Where Parameter tObj = Object or Type
		public void AddConstruction(object tObj, int tFloorId)
		{
			Constructions.Add(tObj, tFloorId);
			Activate();
			//PalleteControl.TreeUpdate(Constructions.CntRootNodes.ToArray())
		}
		//AddConstruction
		//Public Overloads Sub AddConstruction(ByVal tObj As Object, ByRef tFloor As Floor) Handles EventControl.AddConstruction_Event
		//    'If tFloor.FloorId = Me.FloorId Then
		//    Constructions.Add(tObj, tFloor)
		//    Activate()
		//    'End If
		//End Sub 'AddConstruction

		public void DelConstruction(ref ICntTreeViewData tObj)
		{
			var _with1 = (FloorControl)PalleteControl;
			CntTreeNode LastNode = default(CntTreeNode);
			if (_with1.CntTreeView.SelectedNode.PrevNode != null) {
				LastNode = _with1.CntTreeView.SelectedNode.PrevNode;
			} else {
				LastNode = _with1.CntTreeView.SelectedNode.Parent;
			}
			Constructions.RemoveAt(tObj.CntId);
			//PalleteControl.TreeUpdate(Constructions.CntRootNodes.ToArray()) 'UpdateTreeViewControl(Constructions.CntRootNodes)
			_with1.CntTreeView.SelectedNode = LastNode;
			Activate();
			//CntTreeViewSelect(tObj.TypeId, tObj.NodeId - 1)
		}
		//DelConstruction

		public void DetailConstruction(int tCntId)
		{
			//With CType(tObj.PalleteControl, CntDetailControl)
			//    Dim idx As Integer = tObj.TypeId 'DirectCast(tObj, ITreeViewData).TypeId
			//    For i = 0 To Constructions.CntRootNodes(idx).GetNodeCount(True) - 1
			//        .CntComboBox.Items.Add(Constructions.CntRootNodes(idx).Nodes(i).Text)
			//    Next
			//    .CntComboBox.SelectedText = tObj.Header
			//    '.CntComboBox.SelectedIndex = DirectCast(tObj, ITreeViewData).CntId
			//End With
			//Dim CntIdLast As Object = Building.Floors(Me.FloorId).Constructions.FindConctruction(FindOptions.Visible)
			//If Not CntIdLast Is Nothing And tCntId = CntIdLast Then
			if (Building.Floors(this.FloorId).Constructions(tCntId).Visible) {
				PaletteMain.Activate(Constructions(tCntId).TabId);
			} else {
				//CntBuilding.AddTab(Constructions(tCntId), IIf(CntIdLast Is Nothing, Nothing, Constructions(CntIdLast).TabId))
				object CntIdLast = Building.Floors(this.FloorId).Constructions.FindConctruction(FindOptions.Visible);
				CurCnt = Constructions(tCntId);
				CntBuilding.AddTabCnt(tCntId, (CntIdLast == null ? null : Constructions(CntIdLast).TabId));
				PaletteMain.Activate(CurCnt.TabId);
				CurCnt.Current = true;
				//Building.PaletteMain.Add(Constructions(tCntId), True) 'CType(tObj, ITabPalette) CType(Parent, Building).
				//Building.PaletteMain.OnActivate(Constructions(tCntId).TabId) 'CType(tObj, ITabPalette).TabId
			}
		}
		//DetailConstruction

		public void ZoomConstruction(int tCntId)
		{
			DBObject obj = null;
			using (Transaction tr = Db.TransactionManager.StartTransaction()) {
				obj = tr.GetObject(CmdManager.GetObjXData(tCntId).Item(0), OpenMode.ForRead);
			}
			CmdManager.Zoom(obj);
			//obj.Bounds.Value.MinPoint.X, obj.Bounds.Value.MinPoint.Y, obj.Bounds.Value.MaxPoint.X, obj.Bounds.Value.MaxPoint.Y
		}

		public void RotateCnt(SelectOptions tOption, int tCntId)
		{
			CmdManager.RotateEntity(tCntId, tOption);
		}

		public void AboutConstruction(SelectOptions tOption, int tCntId)
		{
			CmdManager.GetObjXData(tCntId, tOption);
		}
		public int CountConstructions()
		{
			//Dim Beam As New Beam(New Point2d(0, 0), New Point2d(0, 0))
			return 1;
			//CntConstructions.Count()
		}

		public void AddCnt<T>(T x, Type y)
		{
			//Dim Args() As Object = {New Point2d}
			//' Get info for an empty constructor for the type.
			//Dim selected_type As System.Type = tObj.GetType()
			//Dim constructor_info As ConstructorInfo
			//constructor_info = selected_type.GetConstructor(System.Type.EmptyTypes)
			//' Invoke the constructor.
			//Dim obj As Object = constructor_info.Invoke(Nothing)
			//Cursor.Current = New Cursor("MyWait.cur")
			//Dim oCnt = Activator.CreateInstance(CType(y, Type))
		}
		#endregion

		#region "Methods of Class"
		public void Activate(object tObj = null)
		{
			this.Current = true;
			Building.Floors.CurFLoorId = this.FloorId;
			PaletteMain.MainPaletteSet(this.TabId).Name = this.Header;

			return;
			var _with2 = (FloorControl)PalleteControl;
			_with2.FloorsComboBox.Items.Clear();
			foreach (Floor floor in Building.Floors) {
				_with2.FloorsComboBox.Items.Add(floor.Header);
			}
			if (_with2.FloorsComboBox.Items.Count > 0) {
				_with2.FloorsComboBox.SelectedIndex = 0;
			}

			var _with3 = (FloorControl)PalleteControl;
			//.CntTreeView.BeginUpdate()
			_with3.CntTreeView.ImageList = CntCollection.CntImgList;
			_with3.CntTreeView.Nodes.Clear();
			//.CntTreeView.Nodes.AddRange(Constructions.CntRootNodes.ToArray())
			if (_with3.CntTreeView.SelectedNode != null) {
				_with3.CntTreeView.ItemHeight = _with3.CntTreeView.SelectedNode.Bounds.Height + 2;
			}
			//.CntTreeView.EndUpdate()
		}
		void ICntTabPalette.OnActivate(object tObj = null)
		{
			Activate(tObj);
		}
		//Activate

		public void Deactivate()
		{
			this.Current = false;
			this.PalleteControl = null;
		}
		void ICntTabPalette.OnDeactivate()
		{
			Deactivate();
		}

		public void Reload(object value)
		{
			if (value != null) {
				value.PalleteControl.pGrid.SelectedObject = value;
				value.PalleteControl.pGrid.PropertySort = Windows.Forms.PropertySort.NoSort;
				PaletteMain.Activate(value.TabId);
				//PaletteSetCollection.MainPaletteSet(CType(value, ITabPalette).TabId).Name = CType(value, ITabPalette).Header
			}
		}

		public void RemoveTab(int tTabId)
		{
			PaletteMain.RemoveAt(tTabId);
		}

		public override bool LoadDataDefault(string tBlockName = "")
		{
			base.LoadDataDefault(FieldsName.ToList, FieldsVal.ToList, FieldsType.ToList, FieldsPmt.ToList, FieldsCmt.ToList, FieldsFormat.ToList);
		}

		public void Close()
		{
			RemoveTab(this.TabId);
			this.Visible = false;

			if (Building.Floors.CurFLoorId == this.FloorId) {
				PaletteMain.Activate(0);
			} else {
				Floor FloorLast = Building.Floors.FindFloor(FindOptions.Visible);
				if (FloorLast != null) {
					Building.Floors.CurFLoorId = FloorLast.FloorId;
					PaletteMain.Activate(FloorLast.TabId);
				} else {
					Building.Floors.CurFLoorId = StatusOptions.None;
				}
			}
			this.Dispose();
		}
		//Close

		public object f(Type t)
		{
			return t.GetConstructor(new Type[]).Invoke(new object[]);
		}

		public void Refresh()
		{
		}

		public new void OnDelete()
		{
			//If TabId > 1 Then TabId -= 1
		}

		public object isCloned()
		{
			return false;
			//' ''CloneId <> CntId
		}

		public object Clone()
		{
			Floor temp = new Floor("") { CloneId = this.FloorId };
			return temp;
		}

		private void NodeMouseEnter(System.Object sender, System.Windows.Forms.TreeNodeMouseClickEventArgs e)
		{
			//If e.Node.Level <> 0 Then
			//    Dim x = e.Node.Bounds.X
			//    Dim y = e.Node.Bounds.Y
			//    Dim h = e.Node.Bounds.Y + e.Node.Bounds.Height
			//    If e.X > x - 18 And e.X < x - 3 Then
			//        If e.Y > y And e.Y < h Then
			//            If e.Node.Nodes.Count = 0 Then
			//                e.Node.ToolTipText = " Form"
			//                ' MsgBox(e.Node.Text + " Form")
			//            End If
			//        End If
			//    ElseIf e.X > x - 35 And e.X < x - 18 Then
			//        If e.Y > y And e.Y < h Then
			//            e.Node.ToolTipText = " Document"
			//            'MsgBox(e.Node.Text + " Document")
			//        End If
			//    End If
			//End If
		}

		protected void OnDrawNode(System.Object sender, System.Windows.Forms.DrawTreeNodeEventArgs e)
		{
			//If e.Node.Level <> 0 Then
			//    With EventControl
			//        Dim indices As List(Of Integer) = TryCast(e.Node.Tag, List(Of Integer))
			//        Dim iconLeft As Integer = 0
			//        For Each i As Integer In indices
			//            e.Graphics.DrawImageUnscaled(.ImgList.Images(i), iconLeft, 0, 16, 16)
			//            iconLeft += 16
			//        Next
			//        Dim p As New Drawing.Point((indices.Count + 1) * 16, 0)
			//        e.Graphics.DrawString(e.Node.Text, e.Node.NodeFont, Drawing.Brushes.Black, p)
			//        ''MyBase.OnDrawNode(e) 
			//    End With
			//End If
		}
		#endregion

	}
}
