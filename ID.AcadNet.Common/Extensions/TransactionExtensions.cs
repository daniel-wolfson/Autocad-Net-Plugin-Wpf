using Autodesk.AutoCAD.DatabaseServices;

namespace Intellidesk.AcadNet.Common.Extentions
{
    public static class TransactionExtensions
    {
        /// <summary> If top transaction then commit And dispose </summary>
        public static void IfNonTopTransactionComminAndClose(Transaction tr)
        {
            if (tr.TransactionManager.TopTransaction == null)
            {
                tr.Commit();
                tr.Dispose();
            }
        }
    }
}