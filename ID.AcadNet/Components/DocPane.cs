using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Windows;
using Intellidesk.AcadNet.Common.Utils;
using App = Autodesk.AutoCAD.ApplicationServices.Application;

namespace Intellidesk.AcadNet.Components
{
	public class DocPane
	{
		private Pane _pane;
		private Document _doc;
		private bool _pushed = true;
		
		public Document Document
		{
			get
			{
				return _doc;
			}
		}
		
		private DocPane() { }
		
		public DocPane(Document doc)
		{
			try
			{
				_doc = doc;
				_pane = new Pane();
				_pane.Enabled = true;
				_pane.Visible = true;
				_pane.Style = PaneStyles.Normal;
				_pane.Text = "DocPane";
				_pane.ToolTipText = "Custom document pane";
				_pane.MouseDown += new StatusBarMouseDownEventHandler(callback_MouseDown);
			    App.StatusBar.Panes.Add(_pane);
			}
			catch(System.Exception ex)
			{
			    Utils.ErrorMessage(ex.Message, "Exception");
			}
		}

		public void Remove()
		{
			try
			{
				_pane.MouseDown -= callback_MouseDown;
			    App.StatusBar.Panes.Remove(_pane);
			}
			catch(System.Exception ex)
			{
			    Utils.ErrorMessage(ex.Message, "Exception");
			}
		}

		private void callback_MouseDown(object sender, StatusBarMouseDownEventArgs e)
		{
			try
			{
				string msg = string.Format("Button: {0}\nDoubleClick: {1}\nCursorPos: {2},{3}",
					e.Button,
					e.DoubleClick,
					e.X, e.Y);
			
				_pane.Style = (_pushed == true ? PaneStyles.PopOut : PaneStyles.Normal);
				_pushed = !_pushed;
			    App.StatusBar.Update();
			    Utils.InfoMessage(msg, "DocPane - MouseDown");
			}
			catch(System.Exception ex)
			{
			    Utils.ErrorMessage(ex.Message, "Exception");
			}
		}
	}
}
