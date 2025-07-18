using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.IO;

namespace Intellidesk.Data
{
    public class RenameCreateDatabaseIfModelChanged<TContext> : IDatabaseInitializer<TContext> where TContext : System.Data.Entity.DbContext
    {
        public void InitializeDatabase(TContext context)
        {
            int version = 1;
            DbCommand cmd;

            if (context.Database.Exists())
            {
                bool throwIfNoMetadata = true;
                if (context.Database.CompatibleWithModel(throwIfNoMetadata))
                {
                    return;
                }

                DbDataReader dr;

                context.Database.Connection.Open();

                //GET VERSION
                cmd = context.Database.Connection.CreateCommand();
                cmd.CommandText = "SELECT TOP 1 * FROM sysobjects WHERE xtype='U' AND name = '__ase.version'";
                dr = cmd.ExecuteReader();
                if (dr.Read())
                {
                    //VERSION EXISTS
                    dr.Close();

                    cmd.CommandText = "SELECT TOP 1 Vesion FROM [__ase.version] ORDER BY CreatedOn DESC";
                    dr = cmd.ExecuteReader();
                    if (dr.Read())
                        version = (int)dr["Vesion"];
                    dr.Close();

                    version++;
                }
                else
                {
                    //First
                    dr.Close();

                    cmd.CommandText = "CREATE TABLE [__ase.version] ([Vesion] [int] NOT NULL, [CreatedOn] [datetime] NOT NULL)";
                    cmd.ExecuteNonQuery();
                    //WriteVersion(context, version);
                }

                //Get list files
                List<string> files = new List<string>();
                cmd.CommandText = "EXEC SP_HELPFILE";
                dr = cmd.ExecuteReader();
                while (dr.Read())
                {
                    files.Add(dr["filename"].ToString());
                }
                dr.Close();

                //Disconnect all connections
                cmd.CommandText = String.Format("ALTER DATABASE [{0}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE", context.Database.Connection.Database);
                cmd.ExecuteNonQuery();
                cmd.CommandText = String.Format("ALTER DATABASE [{0}] SET MULTI_USER WITH ROLLBACK IMMEDIATE", context.Database.Connection.Database);
                cmd.ExecuteNonQuery();

                string dbName = context.Database.Connection.Database;
                //Deattach database
                cmd.CommandText = String.Format("USE MASTER; EXEC SP_DETACH_DB [{0}]", dbName);
                cmd.ExecuteNonQuery();

                //Copy database
                string sql_file_old = String.Format("EXEC SP_ATTACH_DB [{0}], ", dbName);
                string sql_file_new = String.Format("EXEC SP_ATTACH_DB [{0}.{1}], ", dbName, version);
                foreach (string file in files)
                {
                    File.Copy(file, file + "_" + version);

                    sql_file_old += "'" + file + "', ";
                    sql_file_new += "'" + file + "_" + version + "', ";
                }

                //Attach database
                cmd.CommandText = sql_file_old.Substring(0, sql_file_old.Length - 2);
                cmd.ExecuteNonQuery();
                //Attach copy database
                cmd.CommandText = sql_file_new.Substring(0, sql_file_new.Length - 2);
                cmd.ExecuteNonQuery();
                
                context.Database.Connection.Close();
            }

            //Migrate with data loss
            var configuration = new DbMigrationsConfiguration<TContext>();
            configuration.AutomaticMigrationDataLossAllowed = true;
            configuration.AutomaticMigrationsEnabled = true;
            var migrator = new DbMigrator(configuration);
            migrator.Update();

            //Update version
            context.Database.Connection.Open();

            cmd = context.Database.Connection.CreateCommand();
            DbParameter param = cmd.CreateParameter();
            param.ParameterName = "<hh user=v1>";
            param.Value = version;
            cmd.Parameters.Add(param);

            param = cmd.CreateParameter();
            param.ParameterName = "<hh user=v2>";
            param.Value = DateTime.Now;
            cmd.Parameters.Add(param);

            cmd.CommandText = "INSERT INTO [__ase.version] ([Vesion], [CreatedOn]) VALUES (<hh user=v1>, <hh user=v2>)";
            cmd.ExecuteNonQuery();

            context.Database.Connection.Close();
        }

        protected virtual void Seed(TContext context)
        {
        }
    }
}