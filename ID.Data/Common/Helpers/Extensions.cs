using System;
using System.Configuration;
using System.Data.Entity;
using System.Data.SqlClient;

namespace Intellidesk.Data.Common.Helpers
{
    public static class DatabaseExtensions
    {
        public static void BackUp(this Database db)
        {
            var connectionString = db.Connection.ConnectionString;
            // read connectionstring from config file
            //var connectionString = ConfigurationManager.ConnectionStrings["MyConnString"].ConnectionString;

            // read backup folder from config file ("C:/temp/")
            var backupFolder = ConfigurationManager.AppSettings["BackupFolder"];

            //ConfigurationManager<AppDbContext>.ConnectionString
            var sqlConStrBuilder = new SqlConnectionStringBuilder(connectionString);

            // set backupfilename (you will get something like: "C:/temp/MyDatabase-2013-12-07.bak")
            var backupFileName = string.Format("{0}{1}-{2}.bak",
                backupFolder, sqlConStrBuilder.InitialCatalog,
                DateTime.Now.ToString("yyyy-MM-dd"));

            using (var connection = new SqlConnection(sqlConStrBuilder.ConnectionString))
            {
                var query = String.Format("BACKUP DATABASE {0} TO DISK='{1}'",
                    sqlConStrBuilder.InitialCatalog, backupFileName);

                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public static void DatabaseBackUp(Configuration config)
        {
            try
            {
                //var connectionString = db.Connection.ConnectionString;
                // read connectionstring from config file
                //var connectionString = ConfigurationManager.ConnectionStrings["MyConnString"].ConnectionString;

                // read backup folder from config file ("C:/temp/")
                var backupFolder = "C:/temp/"; //config.AppSettings.Settings["BackupFolder"].Value;

                var connectionString = config.ConnectionStrings.ConnectionStrings["AppDbContext"].ConnectionString;
                //var sqlConStrBuilder = new SqlConnectionStringBuilder(connectionString);

                // set backupfilename (you will get something like: "C:/temp/MyDatabase-2013-12-07.bak")
                var backupFileName = string.Format("{0}{1}-{2}.bak", backupFolder, "AcadNet",
                    //backupFolder, sqlConStrBuilder.InitialCatalog,
                    DateTime.Now.ToString("yyyy-MM-dd"));

                using (var connection = new SqlConnection(connectionString))
                {
                    var query = string.Format("BACKUP DATABASE {0} TO DISK='{1}'", "AcadNet", backupFileName);
                    //sqlConStrBuilder.InitialCatalog, backupFileName);

                    using (var command = new SqlCommand(query, connection))
                    {
                        connection.Open();
                        command.ExecuteNonQuery();
                    }
                }

            }
            catch (System.Collections.Generic.KeyNotFoundException ex)
            {
                Console.WriteLine("KeyNotFoundException: {0}", ex.Message);
            }
            catch (System.FormatException ex)
            {
                Console.WriteLine("Format exception: {0}", ex.Message);
            }

        }
    }
}
