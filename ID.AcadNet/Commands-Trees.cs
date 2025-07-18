using System.Windows.Forms;
using Autodesk.AutoCAD.Runtime;

namespace Intellidesk.AcadNet
{
    public class Commands_Tree
    {
        [CommandMethod("SSTREE")]
        public void PopulateCustomSheetTree()
        {
            //// Check the state of the paletteset

            //if (ps == null)
            //{
            //    // Then create it
            //    ps = new PaletteSet("Custom Sheet Tree");
            //    userControl = new ProjectExplorerView();
            //    ps.AddVisual("MySheetView", userControl);
            //}
            //ps.Visible = true;

            //// Get the AutoCAD Editor
            //Editor ed = acadApp.DocumentManager.MdiActiveDocument.Editor;

            //// Get the SheetSet Manager
            //AcSmSheetSetMgr mgr = new AcSmSheetSetMgr();

            //// Create a new SheetSet Database
            //AcSmDatabase db = new AcSmDatabase();

            //// Try and load a default DST file...
            //try
            //{
            //    db = mgr.OpenDatabase(
            //        @"C:\Program Files\Autodesk\AutoCAD 2011\Sample\" +
            //        @"Sheet Sets\Architectural\IRD Addition.dst",
            //        true
            //      );
            //}
            //catch (System.Exception ex)
            //{
            //    ed.WriteMessage(ex.ToString());
            //    return;
            //}

            //// Lock the Db for processing
            //db.LockDb(db);

            //AcSmSheetSet ss = db.GetSheetSet();

            //// Create a root item in the TreeView,
            //// the name of the SheetSet

            //TreeNode root = new TreeNode(ss.GetName());
            //var selected = userControl.TreeExplorer.SelectedItem as TreeViewItem;
            //selected.Items.Add(root);

            //ProcessEnumerator(ss.GetSheetEnumerator(), root);
            //db.UnlockDb(db, true);
            //mgr.Close(db);
        }

        //void ProcessEnumerator(IAcSmEnumComponent iter, TreeNode root)
        //{
        //    IAcSmComponent item = iter.Next();
        //    while (item != null)
        //    {
        //        string type = item.GetTypeName();
        //        switch (type)
        //        {
        //            case "AcSmSubset":
        //                try
        //                {
        //                    AcSmSubset subset = (AcSmSubset)item;
        //                    string subName = subset.GetName();

        //                    if (!String.IsNullOrEmpty(subName))
        //                    {
        //                        TreeNode tn = AddTreeNode(root, subName);
        //                        IAcSmEnumComponent enumerator = 
        //                          (IAcSmEnumComponent)subset.GetSheetEnumerator();
        //                        ProcessEnumerator(enumerator, tn);
        //                    }
        //                }
        //                catch { }
        //                break;

        //            case "AcSmSheet":
        //                try
        //                {
        //                    AcSmSheet sh = (AcSmSheet)item;
        //                    string shName = sh.GetName();

        //                    if (!String.IsNullOrEmpty(shName))
        //                    {
        //                        TreeNode tn = AddTreeNode(root, shName);
        //                        IAcSmEnumComponent enumerator =
        //                          (IAcSmEnumComponent)sh.GetSheetViews();
        //                        ProcessEnumerator(enumerator, tn);
        //                    }
        //                }
        //                catch { }
        //                break;

        //            case "AcSmSheetViews":
        //                try
        //                {
        //                    AcSmSheet sh = (AcSmSheet)item;
        //                    string shName = sh.GetName();

        //                    if (!String.IsNullOrEmpty(shName))
        //                    {
        //                        TreeNode tn = AddTreeNode(root, shName);
        //                        IAcSmEnumComponent enumerator =
        //                          (IAcSmEnumComponent)sh.GetSheetViews();

        //                        ProcessEnumerator(enumerator, tn);
        //                    }
        //                }
        //                catch { }
        //                break;

        //            case "AcSmSheetView":
        //                try
        //                {
        //                    AcSmSheetView sv = (AcSmSheetView)item;
        //                    string svName = sv.GetName();

        //                    if (!String.IsNullOrEmpty(svName))
        //                        AddTreeNode(root, svName);
        //                }
        //                catch { }
        //                break;

        //            case "AcSmCustomPropertyValue":
        //                AcSmCustomPropertyValue pv = (AcSmCustomPropertyValue)item;
                        
        //                AddTreeNode(root,
        //                  "Custom property value: " + pv.GetValue().ToString()
        //                );
        //                break;

        //            case "AcSmObjectReference":
        //                AcSmObjectReference or = (AcSmObjectReference)item;

        //                AddTreeNode(root, "Object reference: " + 
        //                    or.GetReferencedObject().GetTypeName()
        //                );
        //                break;

        //            case "AcSmCustomPropertyBag":
        //                try
        //                {
        //                    AcSmCustomPropertyBag bag = (AcSmCustomPropertyBag)item;
        //                    TreeNode tn = AddTreeNode(root, "Custom property bag");
        //                    IAcSmEnumComponent enumerator =
        //                      (IAcSmEnumComponent)bag.GetPropertyEnumerator();
        //                    ProcessEnumerator(enumerator, tn);
        //                }
        //                catch { }
        //                break;

        //            case "AcSmAcDbLayoutReference":
        //                AcSmAcDbLayoutReference lr = (AcSmAcDbLayoutReference)item;
        //                AddTreeNode(root, "Layout reference: " + lr.GetName()
        //                );
        //                break;

        //            case "AcSmFileReference":
        //                AcSmFileReference fr = (AcSmFileReference)item;
        //                AddTreeNode(root,
        //                  "Layout reference: " + fr.GetFileName()
        //                );

        //                break;

        //            case "AcSmAcDbViewReference":
        //                AcSmAcDbViewReference vr = (AcSmAcDbViewReference)item;
        //                AddTreeNode(root,
        //                  "View reference: " + vr.GetName()
        //                );
        //                break;

        //            case "AcSmResources":
        //                try
        //                {
        //                    AcSmResources res = (AcSmResources)item;
        //                    TreeNode tn = AddTreeNode(root, "Resources");
        //                    IAcSmEnumComponent enumerator =
        //                      (IAcSmEnumComponent)res.GetEnumerator();
        //                    ProcessEnumerator(enumerator, tn);
        //                }
        //                catch { }
        //                break;

        //            default:
        //                Document doc = acadApp.DocumentManager.MdiActiveDocument;
        //                Editor ed = doc.Editor;
        //                ed.WriteMessage("\nMissed Type = " + type);
        //                break;
        //        }
        //        item = iter.Next();
        //    }
        //}

        private TreeNode AddTreeNode(TreeNode root, string name)
        {
            // Create a new node on the tree view
            TreeNode node = new TreeNode(name);

            // Add it to what we have
            root.Nodes.Add(node);

            return node;
        }
    }
}
