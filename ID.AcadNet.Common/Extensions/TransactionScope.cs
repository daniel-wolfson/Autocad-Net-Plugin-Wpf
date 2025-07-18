using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using System;
using acadApp = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using TransactionManager = Autodesk.AutoCAD.DatabaseServices.TransactionManager;

namespace Intellidesk.AcadNet.Common.Extensions
{
    public class TransactionScope : Transaction, IDisposable
    {
        public readonly Transaction tr;
        private readonly DocumentLock _documentLock;
        public readonly OpenMode openMode;
        public bool isAbort;
        public bool ExistTopTransaction;

        public TransactionScope(Database db, OpenMode openMode = OpenMode.ForRead) : base(new IntPtr(), true)
        {
            var doc = acadApp.DocumentManager.GetDocument(db);
            ExistTopTransaction = db.TransactionManager.TopTransaction != null;
            tr = db.TransactionManager.TopTransaction ?? db.TransactionManager.StartTransaction();
            _documentLock = doc.LockDocument();
            this.openMode = openMode;
        }

        /// <summary> Transaction abort </summary>
        public void Abort()
        {
            isAbort = true;
            tr.Abort();
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects).
                    if (!ExistTopTransaction)
                    {
                        if (!isAbort && tr.TransactionManager.NumberOfActiveTransactions > 0)
                            tr.Commit();

                        tr.Dispose();
                    }
                    _documentLock.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // TODO: override a finalizer only if Dispose(bool disposing) above has code to free unmanaged resources.
        // ~AcadTransactionScope() {
        //   // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
        //   Dispose(false);
        // }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion

    }

    public class TransactionManagerScope : TransactionManager
    {
        protected TransactionScope transactionScope;
        public TransactionManagerScope(TransactionManager transactionManager) : base(transactionManager.UnmanagedObject, true)
        {
            transactionScope = transactionScope ?? new TransactionScope(HostApplicationServices.WorkingDatabase, OpenMode.ForRead);
        }
    }

}