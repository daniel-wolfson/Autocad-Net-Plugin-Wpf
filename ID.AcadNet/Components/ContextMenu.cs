using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Windows;
using Intellidesk.AcadNet.Common.Utils;

namespace Intellidesk.AcadNet.Components
{
	public class ContextMenuItem : MenuItem
	{
		private string _command;

		public string Command
		{
			get
			{
				return _command;
			}
		}
		
		public ContextMenuItem(string title, string command)
		: base(title)
		{
			_command = command;
		}
	}

	public class ContextMenu
	{
		private static ContextMenuExtension _appMenu = null;
		
		private ContextMenu() { }

		public static void Add()
		{
			try
			{
				if (_appMenu != null)
				{
					return;
				}
				_appMenu = new ContextMenuExtension();

				_appMenu.Title = "StatusBar";
				_appMenu.MenuItems.Add(new ContextMenuItem("Show Drawing Status Bars", "ShowDrawingStatusBars"));
				_appMenu.MenuItems.Add(new ContextMenuItem("Hide Drawing Status Bars", "HideDrawingStatusBars"));

				foreach(MenuItem item in _appMenu.MenuItems)
				{
					item.Click += new EventHandler(callback_Click);
				}
				Application.AddDefaultContextMenuExtension(_appMenu);
			}	
			catch(System.Exception ex)
			{
				Utils.ErrorMessage(ex.Message, "Exception");
			}
		}

		public static void Remove()
		{
			try
			{
				Application.RemoveDefaultContextMenuExtension(_appMenu);
			}
			catch(System.Exception ex)
			{
				Utils.ErrorMessage(ex.Message, "Exception");
			}
		}

		private static void callback_Click(object sender, EventArgs e)
		{
			try
			{
				ContextMenuItem item = sender as ContextMenuItem;

				string cmd = string.Format("_{0}\n", item.Command);
				Application.DocumentManager.MdiActiveDocument.SendStringToExecute(cmd, false, false, true);
			}
			catch(System.Exception ex)
			{
				Utils.ErrorMessage(ex.Message, "Exception");
			}
		}
	}
}
