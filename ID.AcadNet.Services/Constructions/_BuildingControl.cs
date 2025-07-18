using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Intellidesk.AcadNet.Tools.Constructions
{
    class _BuildingControl
    {
    }
}


//using Microsoft.VisualBasic;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Data;
//using System.Diagnostics;
//using System.ComponentModel;
//using System.Text;

//using Autodesk.AutoCAD.ApplicationServices;
//using Autodesk.AutoCAD.DatabaseServices;
//using Autodesk.AutoCAD.Geometry;
//using Autodesk.AutoCAD.EditorInput;

//using cb.UtilAcd;

[DefaultProperty("Plate:")]
public class BuildingControl
{
	private BaseDataManager dm;

	private object pm;
	#region "Raise Events"
	//'Raise an event.
	public static event Go_EventEventHandler Go_Event;
	public static delegate void Go_EventEventHandler();
	private void GoEvent()
	{
		if (Go_Event != null) {
			Go_Event();
		}
		//BtnAdd.Enabled = True
	}

	public static event Open_EventEventHandler Open_Event;
	public static delegate void Open_EventEventHandler();
	private void OpenEvent()
	{
		if (Open_Event != null) {
			Open_Event();
		}
	}

	public static event Save_EventEventHandler Save_Event;
	public static delegate void Save_EventEventHandler();
	private void SaveEvent()
	{
		if (Save_Event != null) {
			Save_Event();
		}
	}

	public event Reg_EventEventHandler Reg_Event;
	public delegate void Reg_EventEventHandler(bool tCheckBox);
	public event View_EventEventHandler View_Event;
	public delegate void View_EventEventHandler();
	public event AddFloor_EventEventHandler AddFloor_Event;
	public delegate void AddFloor_EventEventHandler();
	public event Close_EventEventHandler Close_Event;
	public delegate void Close_EventEventHandler();
	public event FloorDblClick_EventEventHandler FloorDblClick_Event;
	public delegate void FloorDblClick_EventEventHandler(int tItemIndex);
	public event FloorSelectIndex_EventEventHandler FloorSelectIndex_Event;
	public delegate void FloorSelectIndex_EventEventHandler(int tItemIndex);

	#endregion

	public BuildingControl()
	{
		InitializeComponent();
		//ToolStripButtonGoTo.Image = BaseDataManager.ImageRotate(ToolStripButtonGoTo.Image, Drawing.RotateFlipType.Rotate180FlipNone)
		//ToolStripButtonGoTo.Image = BaseDataManager.MaskFromImage(ToolStripButtonGoTo.Image)
	}

	#region "Events from Buttons"
	private void BtnGo_Click(System.Object sender, System.EventArgs e)
	{
		GoEvent();
	}
	private void BtnOpen_Click(System.Object sender, System.EventArgs e)
	{
		if (Open_Event != null) {
			Open_Event();
		}
	}
	private void BtnSave_Click(System.Object sender, System.EventArgs e)
	{
		if (Save_Event != null) {
			Save_Event();
		}
	}

	private void BtnReg_Click(System.Object sender, System.EventArgs e)
	{
		//RaiseEvent Reg_Event()
		//Dim formOpt As New FormTools
		//formOpt.ShowDialog()
		//formOpt = Nothing
	}
	#endregion

	private void CheckBoxProject_Click(System.Object sender, System.EventArgs e)
	{
		//RaiseEvent Reg_Event(CheckBoxProject.Checked)
	}

	private void BtnView_Click(System.Object sender, System.EventArgs e)
	{
		if (View_Event != null) {
			View_Event();
		}
	}


	private void CheckBoxProject_CheckedChanged(System.Object sender, System.EventArgs e)
	{
	}


	private void LabelCnt_Click(System.Object sender, System.EventArgs e)
	{
	}

	private void NewToolStripButton_Click(System.Object sender, System.EventArgs e)
	{
		if (AddFloor_Event != null) {
			AddFloor_Event();
		}
	}

	private void CloseToolStripButton_Click(System.Object sender, System.EventArgs e)
	{
		//RaiseEvent Close_Event()
	}

	private void ListViewFloors_DoubleClick(System.Object sender, System.EventArgs e)
	{
		if (FloorDblClick_Event != null) {
			FloorDblClick_Event(((System.Windows.Forms.ListView)sender).SelectedIndices(0));
		}
	}

	private void ListViewFloors_SelectedIndexChanged(System.Object sender, System.EventArgs e)
	{
		if (ListViewFloors.SelectedItems.Count > 0) {
			if (FloorSelectIndex_Event != null) {
				FloorSelectIndex_Event(ListViewFloors.SelectedItems(0).Index);
			}
		}
	}
}
