using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Transactions;
using Intellidesk.Data.Models;
using Intellidesk.Data.Models.DataContext;

namespace Intellidesk.Data.Common.Helpers
{
    public class DropCreateDatabaseTables : IDatabaseInitializer<IntelliDesktopContext>
    {
        #region IDatabaseInitializer<Context> Members

        public void InitializeDatabase(IntelliDesktopContext context)
        {
            bool dbExists;
            using (new System.Transactions.TransactionScope(TransactionScopeOption.Suppress))
            {
                dbExists = context.Database.Exists();
            }
            if (dbExists)
            {
                // remove all tables
                context.Database.ExecuteSqlCommand("EXEC sp_MSforeachtable @command1 = \"DROP TABLE ?\"");

                // create all tables
                var dbCreationScript = ((IObjectContextAdapter)context).ObjectContext.CreateDatabaseScript();
                context.Database.ExecuteSqlCommand(dbCreationScript);

                Seed(context);
                context.SaveChanges();

                //var alterCommand = new StringBuilder();
                //string dbName = context.Database.Connection.Database;
                //alterCommand.AppendFormat("alter database [{0}] collate SQL_Latin1_General_CP1251_CI_AS;", dbName);
                //context.Database.ExecuteSqlCommand(alterCommand.ToString());
            }
            else
            {
                throw new ApplicationException("No database instance");
            }
        }

        #endregion

        #region Methods

        protected virtual void Seed(IntelliDesktopContext context)
        {
            /// TODO: put here your seed creation
        }

        #endregion
    }
}
