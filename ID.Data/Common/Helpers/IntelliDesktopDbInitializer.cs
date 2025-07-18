using System.Data.Entity;
using Intellidesk.Data.Models;
using Intellidesk.Data.Models.DataContext;

namespace Intellidesk.Data.Common.Helpers
{
    public class IntelliDesktopDbInitializer : IDatabaseInitializer<IntelliDesktopContext>
    {
        public void InitializeDatabase(IntelliDesktopContext context)
        {
            if (context.Database.Exists())
                context.Database.Delete();
            context.Database.Create();

            //var alterCommand = new StringBuilder();
            //string dbName = context.Database.Connection.Database;
            //alterCommand.AppendFormat("alter database [{0}] collate SQL_Latin1_General_CP1251_CI_AS;", dbName);
            //context.Database.ExecuteSqlCommand(alterCommand.ToString());
        }
    }
}