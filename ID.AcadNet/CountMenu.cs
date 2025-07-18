using System;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Runtime;

namespace Intellidesk.AcadNet
{
    public class CountMenu
    {
        private static Autodesk.AutoCAD.Windows.ContextMenuExtension cme;

        public static void Attach()
        {
            cme = new Autodesk.AutoCAD.Windows.ContextMenuExtension();
            cme.Popup += cme_Popup;
            Autodesk.AutoCAD.Windows.MenuItem mi = new Autodesk.AutoCAD.Windows.MenuItem("My BlockReference menu item");
            mi.Click += new EventHandler(PrintBlockRefMsg);
            cme.MenuItems.Add(mi);
            RXClass rxc = Entity.GetClass(typeof(BlockReference));
            //Application.AddObjectContextMenuExtension(rxc, cme);

            cme = new Autodesk.AutoCAD.Windows.ContextMenuExtension();
            mi = new Autodesk.AutoCAD.Windows.MenuItem("My Table menu item");
            mi.Click += new EventHandler(PrintTableMsg);
            cme.MenuItems.Add(mi);
            rxc = Entity.GetClass(typeof(Table));
            //App.AddObjectContextMenuExtension(rxc, cme);
        }

        static void cme_Popup(object sender, EventArgs e)
        {
            Autodesk.AutoCAD.Windows.ContextMenuExtension cme = (Autodesk.AutoCAD.Windows.ContextMenuExtension)sender;
            Autodesk.AutoCAD.ApplicationServices.Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PromptSelectionResult result = ed.GetSelection();
            String className = result.Value.GetObjectIds().FirstOrDefault().ObjectClass.Name;
            Boolean isEnadled = className == "AcDbBlockReference";

            // Первый вариант: управление доступностью элементов без их удаления/добавления.
            //foreach (MenuItem item in cme.MenuItems)
            //    item.Enabled = isEnadled;

            // Второй вариант: динамическое удаление/добавление элементов меню.
            if (isEnadled && cme.MenuItems.Count == 0)
            {
                Autodesk.AutoCAD.Windows.MenuItem mi = new Autodesk.AutoCAD.Windows.MenuItem("My BlockReference menu item");
                mi.Click += new EventHandler(PrintBlockRefMsg);
                cme.MenuItems.Add(mi);
            }
            else if (!isEnadled)
                foreach (Autodesk.AutoCAD.Windows.MenuItem item in cme.MenuItems.ToArray())
                {
                    item.Click -= new EventHandler(PrintBlockRefMsg);
                    cme.MenuItems.Remove(item);
                }

        }

        public static void Detach()
        {
            RXClass rxc = Entity.GetClass(typeof(Entity));
            //App.RemoveObjectContextMenuExtension(rxc, cme);
        }

        private static void PrintBlockRefMsg(Object o, EventArgs e)
        {
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(
                "Hello from the BlockReference!\n");
        }

        private static void PrintTableMsg(Object o, EventArgs e)
        {
            Application.DocumentManager.MdiActiveDocument.Editor.WriteMessage(
                "Hello from the Table!\n");
        }
    }
}