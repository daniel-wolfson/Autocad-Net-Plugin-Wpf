using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Autodesk.AutoCAD.DatabaseServices;

namespace Intellidesk.AcadNet.Tools.Constructions 
{

	public class CntTreeNode : TreeNode
	{
		public object ParentFloor { get; set; }
		public ICntTreeViewData CntObj { get; set; }
		public Type CntType { get; set; }
		public ObjectId ObjectId { get; set; }
		public string TablName { get; set; }
		public string FieldName { get; set; }
		public int FieldId { get; set; }
		public int NodeId { get; set; }
		public System.Drawing.Image CntImage { get; set; }
		//CType(Resources.GetObject("CopyToolStripButton.Image"), System.Drawing.Image)
		public System.Drawing.Image CntImageNew { get; set; }
		public string Comment { get; set; }
		public int FloorId { get; set; }
		public Hashtable Idx = new Hashtable();
		private int _CntId = 0;

		public new List<CntTreeNode> CntNodes;
		//Public Shadows Function Nodes() As TreeNodeCollection
		//    Return MyBase.Nodes
		//End Function

		public new string Name { get; set; }

		public int CntId {
			get { return _CntId; }
			set {
				_CntId = value;
				try {
					Idx.Add(this.Index, value);
				} catch {
					Idx(this.Index) = value;
				}
			}
		}

		public int NodeIdxFromCntId(int i)
		{
			return Idx(i);
		}

		public CntTreeNode(string tText) : base(tText)
		{
			this.Name = tText;
		}

		//(ByRef tCntType As Type, ByVal tImageList As ImageList, Optional tComment As String = "")
		public CntTreeNode(int tTypeId) : base(CntCollection.CntTypes(tTypeId).Name + "s")
		{
			this.CntType = CntCollection.CntTypes(tTypeId);
			this.NodeId = tTypeId;
			this.Name = CntCollection.CntTypes(tTypeId).Name;
			this.ImageIndex = tTypeId * 2;
			this.SelectedImageIndex = tTypeId * 2;
			this.NodeFont = new Drawing.Font("Microsoft Sans Serif", 9, Drawing.FontStyle.Regular);
			//'Me.Text = Me.Text + IIf(tComment <> "", String.Format("-{0})", tComment), "")
		}

		//Optional ByVal tClone As Boolean = False IIf(tClone, " <- copy from CntId=" + tCntObj.CloneId.ToString, "")
		public CntTreeNode(ref object value, string tComment = "") : base(value.Header)
		{
			var _with1 = (ICntTreeViewData)value;
			//Me.Text = Text + Me.Parent.Nodes.Count.ToString + IIf(tComment <> "", String.Format("({0})", tComment), "")
			this.CntId = _with1.CntId;
			this.CntObj = value;
			this.CntType = _with1.GetType();
			this.ParentFloor = _with1.Parent;
			this.FloorId = _with1.FloorId;
			this.NodeId = _with1.NodeId;
			this.Comment = _with1.Comment;
			this.ImageIndex = _with1.TypeId * 2;
			this.SelectedImageIndex = _with1.TypeId * 2;
			//Me.BackColor = Drawing.Color.Yellow
		}
		//Public Sub New(ByVal tText As String, ByRef tCntType As Type, ByVal tCntTypeImage As System.Drawing.Image, Optional ByVal tTablName As String = "", Optional ByVal tFieldName As String = "", Optional ByVal tFieldId As Integer = 0)
		//    MyBase.New(tText)
		//    CntType = tCntType
		//    Me.Name = tText
		//    Me.NodeFont = New Drawing.Font("Microsoft Sans Serif", 9, Drawing.FontStyle.Bold)
		//    Me.ImageIndex = 0
		//    'Me.CntImage = tCntTypeImage
		//End Sub
	}
}

