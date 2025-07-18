
//
// (C) Copyright 2006 by Autodesk, Inc. 
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted, 
// provided that the above copyright notice appears in all copies and 
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting 
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC. 
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
//
// Use, duplication, or disclosure by the U.S. Government is subject to 
// restrictions set forth in FAR 52.227-19 (Commercial Computer
// Software - Restricted Rights) and DFAR 252.227-7013(c)(1)(ii)
// (Rights in Technical Data and Computer Software), as applicable.
//

using Intellidesk.AcadNet.Common.Utils;
using acadApp = Autodesk.AutoCAD.ApplicationServices;

namespace Intellidesk.AcadNet.Common.Events {

    public class DocumentMgrEvents : EventsBase {

        public
        DocumentMgrEvents()
        {
        }

        protected override void
        EnableEventsImp()
        {
            AcadUi.PrintToCmdLine("\nDocument Manager Events Turned On ...\n");

            acadApp.DocumentCollection docs = acadApp.Application.DocumentManager;

            docs.DocumentActivated += new acadApp.DocumentCollectionEventHandler(event_DocumentActivated);
            docs.DocumentActivationChanged += new acadApp.DocumentActivationChangedEventHandler(event_DocumentActivationChanged);
            docs.DocumentBecameCurrent += new acadApp.DocumentCollectionEventHandler(event_DocumentBecameCurrent);
            docs.DocumentCreated += new acadApp.DocumentCollectionEventHandler(event_DocumentCreated);
            docs.DocumentCreateStarted += new acadApp.DocumentCollectionEventHandler(event_DocumentCreateStarted);
            docs.DocumentCreationCanceled += new acadApp.DocumentCollectionEventHandler(event_DocumentCreationCanceled);
            docs.DocumentDestroyed += new acadApp.DocumentDestroyedEventHandler(event_DocumentDestroyed);
            docs.DocumentLockModeChanged += new acadApp.DocumentLockModeChangedEventHandler(event_DocumentLockModeChanged);
            docs.DocumentLockModeChangeVetoed += new acadApp.DocumentLockModeChangeVetoedEventHandler(event_DocumentLockModeChangeVetoed);
            docs.DocumentLockModeWillChange += new acadApp.DocumentLockModeWillChangeEventHandler(event_DocumentLockModeWillChange);
            docs.DocumentToBeActivated += new acadApp.DocumentCollectionEventHandler(event_DocumentToBeActivated);
            docs.DocumentToBeDeactivated += new acadApp.DocumentCollectionEventHandler(event_DocumentToBeDeactivated);
            docs.DocumentToBeDestroyed += new acadApp.DocumentCollectionEventHandler(event_DocumentToBeDestroyed);
        }

        protected override void
        DisableEventsImp()
        {
            AcadUi.PrintToCmdLine("\nDocument Manager Events Turned Off ...\n");

            acadApp.DocumentCollection docs = acadApp.Application.DocumentManager;

            docs.DocumentActivated -= new acadApp.DocumentCollectionEventHandler(event_DocumentActivated);
            docs.DocumentActivationChanged -= new acadApp.DocumentActivationChangedEventHandler(event_DocumentActivationChanged);
            docs.DocumentBecameCurrent -= new acadApp.DocumentCollectionEventHandler(event_DocumentBecameCurrent);
            docs.DocumentCreated -= new acadApp.DocumentCollectionEventHandler(event_DocumentCreated);
            docs.DocumentCreateStarted -= new acadApp.DocumentCollectionEventHandler(event_DocumentCreateStarted);
            docs.DocumentCreationCanceled -= new acadApp.DocumentCollectionEventHandler(event_DocumentCreationCanceled);
            docs.DocumentDestroyed -= new acadApp.DocumentDestroyedEventHandler(event_DocumentDestroyed);
            docs.DocumentLockModeChanged -= new acadApp.DocumentLockModeChangedEventHandler(event_DocumentLockModeChanged);
            docs.DocumentLockModeChangeVetoed -= new acadApp.DocumentLockModeChangeVetoedEventHandler(event_DocumentLockModeChangeVetoed);
            docs.DocumentLockModeWillChange -= new acadApp.DocumentLockModeWillChangeEventHandler(event_DocumentLockModeWillChange);
            docs.DocumentToBeActivated -= new acadApp.DocumentCollectionEventHandler(event_DocumentToBeActivated);
            docs.DocumentToBeDeactivated -= new acadApp.DocumentCollectionEventHandler(event_DocumentToBeDeactivated);
            docs.DocumentToBeDestroyed -= new acadApp.DocumentCollectionEventHandler(event_DocumentToBeDestroyed);
        }

        private void
        event_DocumentToBeDestroyed(object sender, acadApp.DocumentCollectionEventArgs e)
        {
            PrintEventMessage("Document To Be Destroyed", e.Document);
        }

        private void
        event_DocumentToBeDeactivated(object sender, acadApp.DocumentCollectionEventArgs e)
        {
            PrintEventMessage("Document To Be Deactivated", e.Document);
        }

        private void
        event_DocumentToBeActivated(object sender, acadApp.DocumentCollectionEventArgs e)
        {
            PrintEventMessage("Document To Be Activated", e.Document);
        }

        private void
        event_DocumentLockModeWillChange(object sender, acadApp.DocumentLockModeWillChangeEventArgs e)
        {
            PrintEventMessage("Document Lock Mode Will Change", e.Document);
            if (m_showDetails) {
                PrintSubEventMessage("Global Command Name", e.GlobalCommandName);
                PrintSubEventMessage("Current Mode", e.CurrentMode.ToString());
                PrintSubEventMessage("My Current Mode", e.MyCurrentMode.ToString());
                PrintSubEventMessage("My New Mode", e.MyNewMode.ToString());
            }            
        }

        private void
        event_DocumentLockModeChangeVetoed(object sender, acadApp.DocumentLockModeChangeVetoedEventArgs e)
        {
            PrintEventMessage("Document Lock Mode Change Vetoed", e.Document);
            if (m_showDetails) {
                PrintSubEventMessage("Global Command Name", e.GlobalCommandName);
            }            
        }

        private void
        event_DocumentLockModeChanged(object sender, acadApp.DocumentLockModeChangedEventArgs e)
        {
            PrintEventMessage("Document Lock Mode Changed", e.Document);
            if (m_showDetails) {
                PrintSubEventMessage("Global Command Name", e.GlobalCommandName);
                PrintSubEventMessage("Current Mode", e.CurrentMode.ToString());
                PrintSubEventMessage("My Current Mode", e.MyCurrentMode.ToString());
                PrintSubEventMessage("My Previous Mode", e.MyPreviousMode.ToString());
            }            
        }

        private void
        event_DocumentDestroyed(object sender, acadApp.DocumentDestroyedEventArgs e)
        {
            PrintEventMessage("Document Destroyed", e.FileName);
        }

        private void
        event_DocumentCreationCanceled(object sender, acadApp.DocumentCollectionEventArgs e)
        {
            PrintEventMessage("Document Creation Canceled", e.Document);
        }

        private void
        event_DocumentCreateStarted(object sender, acadApp.DocumentCollectionEventArgs e)
        {
            PrintEventMessage("Document Create Started", e.Document);
        }

        private void
        event_DocumentCreated(object sender, acadApp.DocumentCollectionEventArgs e)
        {
            PrintEventMessage("Document Created", e.Document);
        }

        private void
        event_DocumentBecameCurrent(object sender, acadApp.DocumentCollectionEventArgs e)
        {
            PrintEventMessage("Document Became Current", e.Document);
        }

        private void
        event_DocumentActivationChanged(object sender, acadApp.DocumentActivationChangedEventArgs e)
        {
            PrintEventMessage("Document Activation Changed", e.NewValue.ToString());
        }

        private void
        event_DocumentActivated(object sender, acadApp.DocumentCollectionEventArgs e)
        {
            PrintEventMessage("Document Activated", e.Document);
        }

        #region Print Abstraction

        private void
        PrintEventMessage(string eventStr)
        {
            string printString = string.Format("\n[Doc Mgr Event] : {0,-25}", eventStr);
            AcadUi.PrintToCmdLine(printString);
        }

        private void
        PrintEventMessage(string eventStr, acadApp.Document doc)
        {
            string printString = string.Format("\n[Doc Mgr Event] : {0,-25} : {1}", eventStr, doc.Name);
            AcadUi.PrintToCmdLine(printString);
        }

        private void
        PrintEventMessage(string eventStr, string msg)
        {
            string printString = string.Format("\n[Doc Mgr Event] : {0,-25} : {1}", eventStr, msg);
            AcadUi.PrintToCmdLine(printString);
        }

        private void
        PrintSubEventMessage(string eventStr, string msg)
        {
            string printString = string.Format("\n    {0,-20} : {1}", eventStr, msg);
            AcadUi.PrintToCmdLine(printString);
        }


        #endregion
    }
}
