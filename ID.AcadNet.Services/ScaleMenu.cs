using System;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;

namespace Intellidesk.AcadNet.Services
{
    //add
    //http://through-the-interface.typepad.com/through_the_interface/2014/02/adding-a-context-menu-item-with-an-icon-in-autocad-using-net.html
    //http://through-the-interface.typepad.com/through_the_interface/2014/02/adding-a-context-menu-item-with-an-icon-for-a-specific-autocad-object-type-using-net.html

    public class ScaleMenu
    {
        private static ContextMenuExtension cme;

        public static void Attach()
        {
            if (cme == null)
            {
                cme = new ContextMenuExtension();
                MenuItem mi = new MenuItem("Scale by 5");
                mi.Click += new EventHandler(OnScale);
                cme.MenuItems.Add(mi);
            }
            RXClass rxc = Entity.GetClass(typeof(Entity));
            Application.AddObjectContextMenuExtension(rxc, cme);
        }

        public static void Detach()
        {
            RXClass rxc = Entity.GetClass(typeof(Entity));
            Application.RemoveObjectContextMenuExtension(rxc, cme);
        }

        private static async void OnScale(Object o, EventArgs e)
        {
            var dm = Application.DocumentManager;
            var doc = dm.MdiActiveDocument;
            var ed = doc.Editor;

            // Get the selected objects

            var psr = ed.GetSelection();
            if (psr.Status != PromptStatus.OK)
                return;

            try
            {
                // Ask AutoCAD to execute our command in the right context

                await dm.ExecuteInCommandContextAsync(
                    async (obj) =>
                    {
                        // Scale the selected objects by 5 relative to 0,0,0

                        await ed.CommandAsync(
                            "._scale", psr.Value, "", Point3d.Origin, 5
                            );
                    },
                    null
                    );
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage("\nException: {0}\n", ex.Message);
            }
        }
    }
}