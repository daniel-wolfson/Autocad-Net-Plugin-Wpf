using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;
using Autodesk.AutoCAD.Windows;
using ID.Infrastructure.Extensions;
using Intellidesk.AcadNet.Common.Enums;
using Intellidesk.AcadNet.Common.Extensions;
using Intellidesk.AcadNet.Common.Interfaces;
using Intellidesk.AcadNet.Common.Models;
using Intellidesk.AcadNet.ViewModels;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Size = System.Drawing.Size;

namespace Intellidesk.AcadNet.Views
{

    public class BaseView : UserControl
    {
        private readonly Editor _ed = acadApp.DocumentManager.MdiActiveDocument.Editor;

        protected BaseView()
        {
            //InitializeComponent();
        }

        public BaseView(string info)
        {

        }

        #region "Property"

        public Size MinimumSize { get; set; }
        public Size MaximumSize { get; set; }
        public PaletteViewStatus Status { get; set; }
        public StateEventIndex TabState { get; set; }
        public Palette ParentPalette { get; set; }
        public object ParentPaletteSet { get; set; }
        public bool IsActive { get; set; }
        public bool Current { get; set; }
        public string Title { get; set; }
        public int UniId { get; set; }
        public bool Visible { get; set; }
        public bool Complete { get; set; }
        public string Comment { get; set; }
        public Size Size { get; set; }
        public ICommandArgs ActivateArgument { get; set; }

        #endregion

        #region "events methods"

        private void OnPreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            var item = ItemsControl.ContainerFromElement(sender as ListBox, (DependencyObject)e.OriginalSource) as ListBoxItem;
            if (item != null)
            {
                var objectId = ((ObjectIdItem)item.Content).ObjectId;
                if (objectId != ObjectId.Null)
                {
                    ((SearchTextPanelContext)DataContext).CommandLine.Zoom(((ObjectIdItem)item.Content).ObjectId);
                    ((SearchTextPanelContext)DataContext).ExistListItems.ForEach(x => x.ObjectId.XCast<DBText>().Unhighlight());
                    var ent = objectId.XCast<DBText>();
                    ent.Highlight();
                }
            }
        }

        private void OnDocumentToBeDestroyed(object sender, DocumentCollectionEventArgs e)
        {
            //App.DocumentManager.DocumentToBeDestroyed -= CablePanelDataContext.OnDocumentToBeDestroyed;
            //App.DocumentManager.DocumentActivated -= CablePanelDataContext.OnDocumentActivated;
        }

        #endregion

        #region "Methods"

        public void Apply()
        {
            throw new NotImplementedException();
        }

        public void Refresh(bool flagManualChange = false)
        {
            throw new NotImplementedException();
        }

        public void OnActivate(CommandArgs argument = null)
        {
            // now add the delegate to the events list
            //_ed.PointMonitor += OnInputMonitor;

            // Need to enable the AutoCAD input event mechanism to do a pick under the prevailing
            // pick aperture on all digitizer events, regardless of whether a point is being acquired 
            // or whether any OSNAP modes are currently active.
            //_ed.TurnForcedPickOn();

            // Here we are going to ask the user to pick a point. 
            //PromptPointOptions getPointOptions = new PromptPointOptions("Pick a point: ");
            //PromptPointResult getPointResult = _ed.GetPoint(getPointOptions);
        }

        public void OnDeactivate()
        {
            //_ed.PointMonitor -= OnInputMonitor;
        }

        public void OnInputMonitor(object sender, PointMonitorEventArgs e)
        {
            if (e.Context == null) return;

            //  first lets check what is under the Cursor
            FullSubentityPath[] fullEntPath = e.Context.GetPickedEntities();
            if (fullEntPath.Length > 0)
            {
                //  start a transaction
                Transaction trans = acadApp.DocumentManager.MdiActiveDocument.Database.TransactionManager.StartTransaction();
                try
                {
                    //  open the Entity for read, it must be derived from Curve
                    Curve ent = (Curve)trans.GetObject(fullEntPath[0].GetObjectIds()[0], OpenMode.ForRead);

                    //  ok, so if we are over something - then check to see if it has an extension dictionary
                    if (ent.ExtensionDictionary.IsValid)
                    {
                        // open it for read
                        DBDictionary extensionDict = (DBDictionary)trans.GetObject(ent.ExtensionDictionary, OpenMode.ForRead);

                        // find the entry
                        ObjectId entryId = extensionDict.GetAt("MyData");

                        // if we are here, then all is ok
                        // extract the xrecord
                        Xrecord myXrecord;

                        //  read it from the extension dictionary
                        myXrecord = (Xrecord)trans.GetObject(entryId, OpenMode.ForRead);

                        // We will draw temporary graphics at certain positions along the entity
                        foreach (TypedValue myTypedVal in myXrecord.Data)
                        {
                            if (myTypedVal.TypeCode == (short)DxfCode.Real)
                            {
                                //  To locate the temporary graphics along the Curve 
                                // to show the distances we need to get the point along the curve.
                                Point3d pnt = ent.GetPointAtDist((double)myTypedVal.Value);

                                //  We need to work out how many pixels are in a unit square
                                // so we can keep the temporary graphics a set size regardless of
                                // the zoom scale. 
                                Point2d pixels = e.Context.DrawContext.Viewport.GetNumPixelsInUnitSquare(pnt);

                                //  We need some constant distances. 
                                double xDist = (10 / pixels.X);

                                double yDist = (10 / pixels.Y);

                                // Draw the temporary Graphics. 
                                Circle circle = new Circle(pnt, Vector3d.ZAxis, xDist);

                                e.Context.DrawContext.Geometry.Draw(circle);

                                DBText text = new DBText();

                                // Always a good idea to set the Database defaults With things like 
                                // text, dimensions etc. 
                                text.SetDatabaseDefaults();

                                // Set the position of the text to the same point as the circle, 
                                // but offset by the radius. 
                                text.Position = (pnt + new Vector3d(xDist, yDist, 0));

                                // Use the data from the Xrecord for the text. 
                                text.TextString = myTypedVal.Value.ToString();

                                text.Height = yDist;

                                //  Use the Draw method to display the text. 
                                e.Context.DrawContext.Geometry.Draw(text);


                            }
                        }
                    }
                    //  all ok, commit it
                    trans.Commit();
                }
                catch
                {
                }
                finally
                {
                    //  whatever happens we must dispose the transaction
                    trans.Dispose();
                }
            }
        }


        #endregion
    }
}
