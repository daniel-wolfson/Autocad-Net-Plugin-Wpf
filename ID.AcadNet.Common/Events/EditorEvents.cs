using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Intellidesk.AcadNet.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace Intellidesk.AcadNet.Common.Events
{

    public class EditorEvents : EventsBase
    {

        private List<ObjectId> selectAddedObjects = new List<ObjectId>();
        public bool m_suppressDuringDrag = true;

        public
        EditorEvents()
        {
        }

        protected override void
        EnableEventsImp()
        {
            AcadUi.PrintToCmdLine("\nEditor Events Turned On ...\n");

            DocumentCollection docs = acadApp.DocumentManager;
            foreach (Document doc in docs)
            {
                EnableEvents(doc.Editor);
            }
        }

        public void
        EnableEvents(Editor ed)
        {
            ed.SelectionAdded += new SelectionAddedEventHandler(Event_SelectionAdded);
            ed.SelectionRemoved += new SelectionRemovedEventHandler(Event_SelectionRemoved);

            ed.EnteringQuiescentState += new EventHandler(Event_EnteringQuiescentState);
            ed.LeavingQuiescentState += new EventHandler(Event_LeavingQuiescentState);

            ed.PointFilter += new PointFilterEventHandler(Event_PointFilter);
            ed.PointMonitor += new PointMonitorEventHandler(Event_PointMonitor);

            ed.PromptingForAngle += new PromptAngleOptionsEventHandler(Event_PromptingForAngle);
            ed.PromptedForAngle += new PromptDoubleResultEventHandler(Event_PromptedForAngle);

            ed.PromptingForCorner += new PromptPointOptionsEventHandler(Event_PromptingForCorner);
            ed.PromptedForCorner += new PromptPointResultEventHandler(Event_PromptedForCorner);

            ed.PromptingForDistance += new PromptDistanceOptionsEventHandler(Event_PromptingForDistance);
            ed.PromptedForDistance += new PromptDoubleResultEventHandler(Event_PromptedForDistance);

            ed.PromptingForDouble += new PromptDoubleOptionsEventHandler(Event_PromptingForDouble);
            ed.PromptedForDouble += new PromptDoubleResultEventHandler(Event_PromptedForDouble);

            ed.PromptingForEntity += new PromptEntityOptionsEventHandler(Event_PromptingForEntity);
            ed.PromptedForEntity += new PromptEntityResultEventHandler(Event_PromptedForEntity);
            ed.PromptForEntityEnding += new PromptForEntityEndingEventHandler(Event_PromptForEntityEnding);

            ed.PromptingForInteger += new PromptIntegerOptionsEventHandler(Event_PromptingForInteger);
            ed.PromptedForInteger += new PromptIntegerResultEventHandler(Event_PromptedForInteger);

            ed.PromptingForKeyword += new PromptKeywordOptionsEventHandler(Event_PromptingForKeyword);
            ed.PromptedForKeyword += new PromptStringResultEventHandler(Event_PromptedForKeyword);

            ed.PromptingForNestedEntity += new PromptNestedEntityOptionsEventHandler(Event_PromptingForNestedEntity);
            ed.PromptedForNestedEntity += new PromptNestedEntityResultEventHandler(Event_PromptedForNestedEntity);

            ed.PromptingForPoint += new PromptPointOptionsEventHandler(Event_PromptingForPoint);
            ed.PromptedForPoint += new PromptPointResultEventHandler(Event_PromptedForPoint);

            ed.PromptingForSelection += new PromptSelectionOptionsEventHandler(Event_PromptingForSelection);
            ed.PromptedForSelection += new PromptSelectionResultEventHandler(Event_PromptedForSelection);
            ed.PromptForSelectionEnding += new PromptForSelectionEndingEventHandler(Event_PromptForSelectionEnding);

            ed.PromptingForString += new PromptStringOptionsEventHandler(Event_PromptingForString);
            ed.PromptedForString += new PromptStringResultEventHandler(Event_PromptedForString);
        }

        protected override void
        DisableEventsImp()
        {
            AcadUi.PrintToCmdLine("\nEditor Events Turned Off ...\n");

            DocumentCollection docs = Application.DocumentManager;
            foreach (Document doc in docs)
            {
                DisableEvents(doc.Editor);
            }
        }

        public void
        DisableEvents(Editor ed)
        {
            ed.EnteringQuiescentState -= new EventHandler(Event_EnteringQuiescentState);
            ed.LeavingQuiescentState -= new EventHandler(Event_LeavingQuiescentState);

            ed.PointFilter -= new PointFilterEventHandler(Event_PointFilter);
            ed.PointMonitor -= new PointMonitorEventHandler(Event_PointMonitor);

            ed.PromptingForAngle -= new PromptAngleOptionsEventHandler(Event_PromptingForAngle);
            ed.PromptedForAngle -= new PromptDoubleResultEventHandler(Event_PromptedForAngle);

            ed.PromptingForCorner -= new PromptPointOptionsEventHandler(Event_PromptingForCorner);
            ed.PromptedForCorner -= new PromptPointResultEventHandler(Event_PromptedForCorner);

            ed.PromptingForDistance -= new PromptDistanceOptionsEventHandler(Event_PromptingForDistance);
            ed.PromptedForDistance -= new PromptDoubleResultEventHandler(Event_PromptedForDistance);

            ed.PromptingForDouble -= new PromptDoubleOptionsEventHandler(Event_PromptingForDouble);
            ed.PromptedForDouble -= new PromptDoubleResultEventHandler(Event_PromptedForDouble);

            ed.PromptingForEntity -= new PromptEntityOptionsEventHandler(Event_PromptingForEntity);
            ed.PromptedForEntity -= new PromptEntityResultEventHandler(Event_PromptedForEntity);
            ed.PromptForEntityEnding -= new PromptForEntityEndingEventHandler(Event_PromptForEntityEnding);

            ed.PromptingForInteger -= new PromptIntegerOptionsEventHandler(Event_PromptingForInteger);
            ed.PromptedForInteger -= new PromptIntegerResultEventHandler(Event_PromptedForInteger);

            ed.PromptingForKeyword -= new PromptKeywordOptionsEventHandler(Event_PromptingForKeyword);
            ed.PromptedForKeyword -= new PromptStringResultEventHandler(Event_PromptedForKeyword);

            ed.PromptingForNestedEntity -= new PromptNestedEntityOptionsEventHandler(Event_PromptingForNestedEntity);
            ed.PromptedForNestedEntity -= new PromptNestedEntityResultEventHandler(Event_PromptedForNestedEntity);

            ed.PromptingForPoint -= new PromptPointOptionsEventHandler(Event_PromptingForPoint);
            ed.PromptedForPoint -= new PromptPointResultEventHandler(Event_PromptedForPoint);

            ed.PromptingForSelection -= new PromptSelectionOptionsEventHandler(Event_PromptingForSelection);
            ed.PromptedForSelection -= new PromptSelectionResultEventHandler(Event_PromptedForSelection);
            ed.PromptForSelectionEnding -= new PromptForSelectionEndingEventHandler(Event_PromptForSelectionEnding);
            ed.SelectionAdded -= new SelectionAddedEventHandler(Event_SelectionAdded);
            ed.SelectionRemoved -= new SelectionRemovedEventHandler(Event_SelectionRemoved);

            ed.PromptingForString -= new PromptStringOptionsEventHandler(Event_PromptingForString);
            ed.PromptedForString -= new PromptStringResultEventHandler(Event_PromptedForString);
        }

        public void AddToList(ObjectId oid)
        {
            if (!selectAddedObjects.Contains(oid))
                selectAddedObjects.Add(oid);
        }

        public void RemoveFromList(ObjectId oid)
        {
            if (selectAddedObjects.Contains(oid))
                selectAddedObjects.Remove(oid);
        }

        public bool IsInList(ObjectId oid)
        {
            return selectAddedObjects.Contains(oid);
        }

        private void Event_SelectionRemoved(object sender, SelectionRemovedEventArgs e)
        {
            PrintEventMessage(sender, "Selection Removed");
            /*if (m_showDetails) {
                Snoop.Forms.Objects dbox = new Snoop.Forms.Objects(e);
                dbox.Text = "SelectionRemoved";
                dbox.ShowDialog();
            }*/
        }

        private void Event_SelectionAdded(object sender, SelectionAddedEventArgs e)
        {
            ObjectId id = e.AddedObjects.GetObjectIds().Last();
            using (Transaction tr = ((Editor)sender).Document.Database.TransactionManager.StartTransaction())
            {
                Entity ent = (Entity)tr.GetObject(id, OpenMode.ForRead);
                tr.Commit();
                //lastSelectAddedObjectId = ent.ObjectId;
                PrintEventMessage(sender, $"Selection Added ObjectId: {ent.ObjectId} type of {ent.GetType().Name} dfxName: {ent.ObjectId.ObjectClass.DxfName}");
            }

            //ObjectId[] addedIds = e.AddedObjects.GetObjectIds();
            //for (int i = 0; i < addedIds.Length; i++)
            //{
            //    ObjectId oid = addedIds[i];
            //    if (!IsInList(oid))
            //        AddToList(oid);
            //    //e.Remove(i);
            //}

            //var actualSelectAddedObjectId = e.AddedObjects.GetObjectIds().Last();
            //if (((Editor) sender).IsQuiescent && actualSelectAddedObjectId != lastSelectAddedObjectId)
            //{
            //    lastSelectAddedObjectId = actualSelectAddedObjectId;
            //    PrintEventMessage(sender, "Selection Added");
            //}
            /*if (m_showDetails) {
                Snoop.Forms.Objects dbox = new Snoop.Forms.Objects(e);
                dbox.Text = "SelectionAdded";
                dbox.ShowDialog();
            }*/
        }

        private void Event_PromptingForString(object sender, PromptStringOptionsEventArgs e)
        {
            PrintEventMessage(sender, "Prompting For String");
            /*if (m_showDetails) {
                Snoop.Forms.Objects dbox = new Snoop.Forms.Objects(e);
                dbox.Text = "PromptingForString";
                dbox.ShowDialog();
            }*/
        }

        private void Event_PromptingForSelection(object sender, PromptSelectionOptionsEventArgs e)
        {
            PrintEventMessage(sender, "Prompting For Selection");
            /*if (m_showDetails) {
                Snoop.Forms.Objects dbox = new Snoop.Forms.Objects(e);
                dbox.Text = "PromptingForSelection";
                dbox.ShowDialog();
            }*/
        }

        private void Event_PromptingForPoint(object sender, PromptPointOptionsEventArgs e)
        {
            PrintEventMessage(sender, "Prompting For Point");
            /*if (PrintDetails(sender)) {
                Snoop.Forms.Objects dbox = new Snoop.Forms.Objects(e);
                dbox.Text = "PromptingForPoint";
                dbox.ShowDialog();
            }*/
        }

        private void Event_PromptingForNestedEntity(object sender, PromptNestedEntityOptionsEventArgs e)
        {
            PrintEventMessage(sender, "Prompting For Nested Entity");
            /*if (PrintDetails(sender)) {
                Snoop.Forms.Objects dbox = new Snoop.Forms.Objects(e);
                dbox.Text = "PromptingForNestedEntity";
                dbox.ShowDialog();
            }*/
        }

        private void Event_PromptingForKeyword(object sender, PromptKeywordOptionsEventArgs e)
        {
            PrintEventMessage(sender, "Prompting For Keyword");
            /*if (PrintDetails(sender)) {
                Snoop.Forms.Objects dbox = new Snoop.Forms.Objects(e);
                dbox.Text = "PromptingForKeyword";
                dbox.ShowDialog();
            }*/
        }

        private void Event_PromptingForInteger(object sender, PromptIntegerOptionsEventArgs e)
        {
            PrintEventMessage(sender, "Prompting For Integer");
            /*if (PrintDetails(sender)) {
                Snoop.Forms.Objects dbox = new Snoop.Forms.Objects(e);
                dbox.Text = "PromptingForInteger";
                dbox.ShowDialog();
            }*/
        }

        private void Event_PromptingForEntity(object sender, PromptEntityOptionsEventArgs e)
        {
            PrintEventMessage(sender, "Prompting For Entity");
            /*if (PrintDetails(sender)) {
                Snoop.Forms.Objects dbox = new Snoop.Forms.Objects(e);
                dbox.Text = "PromptingForEntity";
                dbox.ShowDialog();
            }*/
        }

        private void Event_PromptingForDouble(object sender, PromptDoubleOptionsEventArgs e)
        {
            PrintEventMessage(sender, "Prompting For Double");
            /*if (PrintDetails(sender)) {
                Snoop.Forms.Objects dbox = new Snoop.Forms.Objects(e);
                dbox.Text = "PromptingForDouble";
                dbox.ShowDialog();
            }*/
        }

        private void Event_PromptingForDistance(object sender, PromptDistanceOptionsEventArgs e)
        {
            PrintEventMessage(sender, "Prompting For Distance");
            /*if (PrintDetails(sender)) {
                Snoop.Forms.Objects dbox = new Snoop.Forms.Objects(e);
                dbox.Text = "PromptingForDistance";
                dbox.ShowDialog();
            }*/
        }

        private void Event_PromptingForCorner(object sender, PromptPointOptionsEventArgs e)
        {
            PrintEventMessage(sender, "Prompting For Corner");
            /*if (PrintDetails(sender)) {
                Snoop.Forms.Objects dbox = new Snoop.Forms.Objects(e);
                dbox.Text = "PromptingForCorner";
                dbox.ShowDialog();
            }*/
        }

        private void Event_PromptingForAngle(object sender, PromptAngleOptionsEventArgs e)
        {
            PrintEventMessage(sender, "Prompting For Angle");
            /*if (PrintDetails(sender)) {
                Snoop.Forms.Objects dbox = new Snoop.Forms.Objects(e);
                dbox.Text = "PromptingForAngle";
                dbox.ShowDialog();
            }*/
        }

        private void Event_PromptForSelectionEnding(object sender, PromptForSelectionEndingEventArgs e)
        {
            PrintEventMessage(sender, "Prompt For Selection Ending");
            if (m_showDetails)
            {
            }
        }

        private void Event_PromptForEntityEnding(object sender, PromptForEntityEndingEventArgs e)
        {
            PrintEventMessage(sender, "Prompt For Entity Ending");
            if (m_showDetails)
            {
            }
        }

        private void Event_PromptedForString(object sender, PromptStringResultEventArgs e)
        {
            PrintEventMessage(sender, "Prompted For String");
            if (m_showDetails)
            {
            }
        }

        private void Event_PromptedForSelection(object sender, PromptSelectionResultEventArgs e)
        {
            PrintEventMessage(sender, "Prompted For Selection");
            if (m_showDetails)
            {
            }
        }

        private void Event_PromptedForPoint(object sender, PromptPointResultEventArgs e)
        {
            PrintEventMessage(sender, "Prompted For Point");
            if (m_showDetails)
            {
            }
        }

        private void Event_PromptedForNestedEntity(object sender, PromptNestedEntityResultEventArgs e)
        {
            PrintEventMessage(sender, "Prompted For Nested Entity");
            if (m_showDetails)
            {
            }
        }

        private void Event_PromptedForKeyword(object sender, PromptStringResultEventArgs e)
        {
            PrintEventMessage(sender, "Prompted For Keyword");
            if (m_showDetails)
            {
            }
        }

        private void Event_PromptedForInteger(object sender, PromptIntegerResultEventArgs e)
        {
            PrintEventMessage(sender, "Prompted For Integer");
            if (m_showDetails)
            {
            }
        }

        private void Event_PromptedForEntity(object sender, PromptEntityResultEventArgs e)
        {
            PrintEventMessage(sender, "Prompted For Entity");
            if (m_showDetails)
            {
            }
        }

        private void Event_PromptedForDouble(object sender, PromptDoubleResultEventArgs e)
        {
            PrintEventMessage(sender, "Prompted For Double");
            if (m_showDetails)
            {
            }
        }

        private void Event_PromptedForDistance(object sender, PromptDoubleResultEventArgs e)
        {
            PrintEventMessage(sender, "Prompted For Distance");
            if (m_showDetails)
            {
            }
        }

        private void Event_PromptedForCorner(object sender, PromptPointResultEventArgs e)
        {
            PrintEventMessage(sender, "Prompted For Corner");
            if (m_showDetails)
            {
            }
        }

        private void Event_PromptedForAngle(object sender, PromptDoubleResultEventArgs e)
        {
            PrintEventMessage(sender, "Prompted For Angle");
            if (m_showDetails)
            {
            }
        }

        private void Event_PointMonitor(object sender, PointMonitorEventArgs e)
        {
            PrintEventMessage(sender, "Point Monitor");
            if (m_showDetails)
            {
            }
        }

        private void Event_PointFilter(object sender, PointFilterEventArgs e)
        {
            PrintEventMessage(sender, "Point Filter");
            if (m_showDetails)
            {
            }
        }

        private void Event_LeavingQuiescentState(object sender, EventArgs e)
        {
            PrintEventMessage(sender, "Leaving Quiescent State");
        }

        private void Event_EnteringQuiescentState(object sender, EventArgs e)
        {
            PrintEventMessage(sender, "Entering Quiescent State");
        }

        #region Print Abstraction

        private void
        PrintEventMessage(object obj, string eventStr)
        {
            //Editor ed = (Editor)obj;
            //if (!(ed.IsDragging && m_suppressDuringDrag)) {
            string printString = string.Format("\n[Editor Event] : {0,-20} ", eventStr);
            AcadUi.PrintToCmdLine(printString);
            //}
        }

        private bool
        PrintDetails(object obj)
        {
            // TBD: getting so many messages during drag that they become worthless.  On C++ side
            // there was a beginDragSequence() event that allowed us to filter the extras out.  Can't
            // find a way to do that on .NET side (jma - 09/08/06)
            Editor ed = (Editor)obj;
            if (ed.IsDragging)
            {
                // print the first drag message, but suppress the rest until the
                // end of the drag sequence
                //if (m_dragJustStarted) {
                //    m_dragJustStarted = false;
                //    return true;
                return false;
            }
            else
            {
                return m_showDetails;
            }
        }

        #endregion
    }
}
