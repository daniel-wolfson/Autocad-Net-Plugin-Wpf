using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.AutoCAD.Geometry;

namespace Intellidesk.AcadNet.Tools.Constructions
{
	public enum YesNoEmpty
	{
		Empty = -1,
		No = 0,
		Yes = 1
	}

	public abstract class Construction : BaseDataManager, IDisposable, ICntTabPalette, ICntTreeViewData
	{
		public static CntCollection Constructions = new CntCollection();
		//' '' ''Private Shared _Count As Integer = -1
		//Public Shared RootPath As String = Me.BasePath 'Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\" 'Directory.GetCurrentDirectory() & "\"
		#region "Public Property"

	    private string _Name = ""; // ERROR: Not supported in C#: ClassReferenceExpression.GetType.Name.ToLower;
		[Browsable(false), CategoryAttribute("_Temp:"), DisplayName("(PalleteControl)")]
		public virtual object PalleteControl { get; set; }
		[Browsable(false)]
		public virtual int TabId { get; set; }
		[Browsable(false)]
		public virtual object Parent { get; set; }
		[Browsable(false)]
		public virtual int TypeId { get; set; }
		[Browsable(false)]
		public virtual int DataId { get; set; }
		[Browsable(false)]
		public virtual int FloorId { get; set; }
		[Browsable(false)]
		public virtual int CloneId { get; set; }
		[Browsable(false)]
		public virtual bool Deleted { get; set; }
		[Browsable(false)]
		public virtual System.Drawing.Image CntImage { get; set; }
		[Browsable(false)]
		public virtual System.Drawing.Image CntImageNew { get; set; }
		[Browsable(false)]
		public virtual bool Active { get; set; }
		bool ICntTreeViewData.Current {
			get { return Active; }
			set { Active = value; }
		}
		bool ICntTabPalette.Current {
			get { return Active; }
			set { Active = value; }
		}

		[Browsable(true), CategoryAttribute("User:")]
		public virtual string Comment { get; set; }
		[Browsable(true), CategoryAttribute("_Temp:"), ReadOnly(true), DisplayName("(CntId)")]
		public virtual int CntId { get; set; }
		int ICntTabPalette.UniId {
			get { return CntId; }
			set { CntId = value; }
		}
		[Browsable(true), CategoryAttribute("_Temp:"), ReadOnly(true), DisplayName("(NodeId)")]
		public virtual int NodeId { get; set; }
		[Browsable(true), CategoryAttribute("_Temp:"), ReadOnly(true), TypeConverter(typeof(Converters.YesNoClassConverter)), DisplayName("(complete)")]
		public virtual bool Complete { get; set; }
		[Browsable(true), CategoryAttribute("_Temp:"), ReadOnly(true), DisplayName("(Header)")]
		public virtual string Header { get; set; }
		[Browsable(true), CategoryAttribute("_Temp:"), ReadOnly(true), DisplayName("(Visible)")]
		public virtual bool Visible { get; set; }

		[Browsable(true), CategoryAttribute("Data:"), ReadOnly(true), DisplayName("(Lenght)")]
		public virtual double Lenght { get; set; }

		[Browsable(true), CategoryAttribute("Draw elements:"), TypeConverter(typeof(Converters.BorderStyleConverter)), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public virtual int Border { get; set; }
		[Browsable(true), CategoryAttribute("Draw elements:")]
		public virtual System.Drawing.Color BorderColor { get; set; }
		//BorderColor
		[Browsable(true), CategoryAttribute("Draw elements:"), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public virtual int Placement { get; set; }
		[Browsable(true), CategoryAttribute("Draw elements:"), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public virtual System.Drawing.Font Font { get; set; }
		[Browsable(true), CategoryAttribute("Draw elements:"), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public virtual System.Drawing.Color FontColor { get; set; }
		//BorderColor
		[Browsable(true), CategoryAttribute("Draw elements:"), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public virtual bool Axes { get; set; }
		[Browsable(true), CategoryAttribute("Draw elements:"), ReadOnly(false), BindableAttribute(false), DesignOnly(false), DisplayName("Rotation"), DefaultValueAttribute("0"), DescriptionAttribute("Rotation from base start")]
		public virtual int RotationDegree { get; set; }
		//Rotation
		[Browsable(true), CategoryAttribute("Draw elements:"), TypeConverter(typeof(Converters.ConcreteConverter)), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public virtual string Concrete { get; set; }

		[CategoryAttribute("Points:"), DisplayName("First point"), DefaultValueAttribute("0,0"), DescriptionAttribute("Point start"), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public virtual Drawing.Point CntStartPoint { get; set; }
		//point 1

		[CategoryAttribute("Points:"), DisplayName("End point"), DefaultValueAttribute("0,0"), DescriptionAttribute("Point end"), Browsable(true), ReadOnly(false), BindableAttribute(false), DesignOnly(false)]
		public virtual Drawing.Point CntEndPoint { get; set; }
		//point 2

		private List<Drawing.Point> _PntList = new List<Drawing.Point>();
		[Editor(typeof(PointsEditor), typeof(UITypeEditor)), Description("Selected Points Colllection")]
		[Browsable(true), CategoryAttribute("Points:"), DisplayName("Points Select")]
		public virtual List<Drawing.Point> SelectedPoints {
			get { return _PntList; }

			set { }
		}

		public virtual string Name {
			get { return _Name; }
			set { _Name = value; }
		}

		#endregion

		#region "Constuctor"
		public Construction() : base("")
		{
			//Midb.mdb
			this.CloneId =  // ERROR: Not supported in C#: ClassReferenceExpression
.CntId;
		}
		#endregion

		#region "Methods"
		public virtual void Activate(object tObj = null)
		{
			//Debug.WriteLine("Construction Activate")
			if ((tObj != null)) {
				var _with1 = (CntDetailControl)tObj.PalleteControl;
				_with1.CntComboBox.Items.Clear();
				int idx = tObj.TypeId;
				//DirectCast(tObj, ITreeViewData).TypeId
				//For Each nod In Building.Floors(CType(tObj.Parent, Floor).FloorId).Constructions.CntRootNodes(idx).Nodes
				foreach (void nod_loopVariable in Building.Floors(tObj.FloorId).Constructions.CntRootNodes(idx).Nodes) {
					nod = nod_loopVariable;
					//For i = 0 To Building.Floors(CType(tObj.Parent, Floor).FloorId).Constructions.CntRootNodes(idx).GetNodeCount(True) - 1
					//.CntComboBox.Items.Add(Building.Floors(CType(tObj.Parent, Floor).FloorId).Constructions.CntRootNodes(idx).Nodes(i).Text)
					_with1.CntComboBox.Items.Add(nod.Text);
				}
				_with1.CntComboBox.SelectedItem = this.Header;
				//.CntComboBox.SelectedIndex = DirectCast(tObj, ITreeViewData).CntId
			}
		}
		void ICntTabPalette.OnActivate(object tObj = null)
		{
			Activate(tObj);
		}
		//Activate

		public virtual void Deactivate()
		{
		}
		void ICntTabPalette.OnDeactivate()
		{
			Deactivate();
		}

		public virtual List<Drawing.Point> SelectPoints(int tPromptCount)
		{
			PromptPointOptions prPointOptions = default(PromptPointOptions);
			//prPointOptions = New PromptPointOptions(vbCrLf)
			PromptPointResult Pnt = null;
			List<Drawing.Point> PntList = new List<Drawing.Point>();

			for (int i = 1; i <= tPromptCount; i++) {
				prPointOptions = new PromptPointOptions(string.Format(Constants.vbCrLf + "Select a point {0}: ", i));
				Pnt = Ed.GetPoint(prPointOptions);
				PntList.Add(new Drawing.Point(Pnt.Value.X, Pnt.Value.Y));
			}

			if (Pnt.Status == PromptStatus.OK) {
				return PntList;
			} else {
				return null;
			}
		}

		//toCad ParamArray tParam() As Object
		public virtual void OnPaint(byte f = 1, List<Point3d> tPntList = null)
		{
		}

		//Cad
		public virtual void OnRePaint(int f = 0, long color = -1, double width = 2)
		{
		}

		public void AfterUpdate()
		{
		}

		public virtual void RePaint()
		{
			//'To do
		}

		public virtual void Refresh()
		{
		}

		//Public Overridable Sub OnDelete()
		//    'If TabId > 1 Then TabId -= 1
		//End Sub

		public object isCloned()
		{
			return CloneId != CntId;
		}
		//Public Overridable Function Clone() As Object Implements ICloneable.Clone
		//    Dim temp As Beam = New Beam() With {.CloneId = Me.CntId}
		//    Return temp
		//End Function

		#endregion
		#region "Inner Classes" '<TypeConverter(GetType(ExpandableObjectConverter))> _
		//<Editor(GetType(PointsEditor), GetType(UITypeEditor)), Description("Selected Points Colllection")> _
		public class Points
		{
			private List<Drawing.Point> m_bar;
			[DisplayName("Points Collection")]
			public List<Drawing.Point> Points {
				get { return m_bar; }
				set { m_bar = value; }
			}
		}
		#endregion
	}
}

