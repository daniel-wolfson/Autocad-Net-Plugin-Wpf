using Autodesk.AutoCAD.Windows;
using ID.Infrastructure.Commands;
using Intellidesk.AcadNet.Common.Utils;

namespace Intellidesk.AcadNet.Components
{
	public class AppPane
	{
		private Pane _pane;
		private bool _pushed = true;

		public AppPane()
		{
			_pane = new Pane();
			_pane.Enabled = true;
			_pane.Visible = true;
			_pane.Style = PaneStyles.Normal;
			_pane.Text = CommandNames.UserGroup;
			_pane.ToolTipText = "Custom application pane";
			_pane.MouseDown += callback_MouseDown;
			Autodesk.AutoCAD.ApplicationServices.Application.StatusBar.Panes.Add(_pane);
		}

		public void Remove()
		{
			try
			{
				_pane.MouseDown -= callback_MouseDown;
			}
			catch (System.Exception ex)
			{
				Utils.ErrorMessage(ex.Message, "Exception");
			}
		}

		private void callback_MouseDown(object sender, StatusBarMouseDownEventArgs e)
		{
			try
			{
				string msg = $"Button: {e.Button}\nDoubleClick: {e.DoubleClick}\nCursorPos: {e.X},{e.Y}";

				_pane.Style = (_pushed == true ? PaneStyles.PopOut : PaneStyles.Normal);
				_pushed = !_pushed;

				Autodesk.AutoCAD.ApplicationServices.Application.StatusBar.Update();
				Utils.InfoMessage(msg, "AppPane - MouseDown");
			}
			catch (System.Exception ex)
			{
				Utils.ErrorMessage(ex.Message, "Exception");
			}
		}
	}
}
