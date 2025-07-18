using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;

namespace WCFAgentDesktopLogin
{
    class SqlDataAccess : IDisposable
    {
        protected SqlConnection Connection;
        protected DataTable dt = new DataTable();
        private const string FormatError = "Date:{0} Input Parametrs: {1}. Error: {2}";

        public SqlDataAccess()
        {
            dt.Locale = CultureInfo.InvariantCulture;
            Connection = new SqlConnection(ConfigurationManager.AppSettings["conStr"]);
        }


        protected DataTable GetDataTable(SqlCommand cmd)
        {
            try
            {
                Connection.Open();
                using (var da = new SqlDataAdapter(cmd))
                {
                    da.Fill(dt);
                }
            }
            catch (SqlException ex)
            {
                string InputParametrs = string.Empty;
                if (cmd != null)
                {
                    InputParametrs = "Command Text:" + cmd.CommandText + Environment.NewLine;
                    InputParametrs += "Parametrs:" + Environment.NewLine;
                    foreach (SqlParameter item in cmd.Parameters)
                    {
                        InputParametrs += string.Concat(item.ParameterName, ":", item.Value, " Direction:", item.Direction);
                        InputParametrs += Environment.NewLine;

                    }

                }
                Trace.Fail(string.Format(CultureInfo.InvariantCulture, FormatError, DateTime.Now, InputParametrs, ex.ToString()));
            }
            finally
            {
                if (Connection.State != ConnectionState.Closed)
                    Connection.Close();
            }
            return dt;
        }

        protected string GetScalar(SqlCommand cmd)
        {
            string result = string.Empty;
            try
            {
                Connection.Open();
                result = cmd.ExecuteScalar() + "";
            }
            catch (SqlException ex)
            {
                string inputParametrs = string.Empty;
                if (cmd != null)
                {
                    inputParametrs = "Command Text:" + cmd.CommandText + Environment.NewLine;
                    inputParametrs += "Parametrs:" + Environment.NewLine;
                    foreach (SqlParameter item in cmd.Parameters)
                    {
                        inputParametrs += string.Concat(item.ParameterName, ":", item.Value, " Direction:", item.Direction);
                        inputParametrs += Environment.NewLine;

                    }

                }
                Trace.Fail(string.Format(CultureInfo.InvariantCulture, FormatError, DateTime.Now, inputParametrs, ex));
            }
            finally
            {
                if (Connection.State != ConnectionState.Closed)
                    Connection.Close();
            }
            return result;
        }

        protected bool ExecuteNonQuery(SqlCommand com)
        {
            bool result = false;
            try
            {
                Connection.Open();
                com.ExecuteNonQuery();
                result = true;
            }
            catch (SqlException ex)
            {
                var inputParametrs = string.Empty;
                if (com != null)
                {
                    inputParametrs = "Command Text:" + com.CommandText + Environment.NewLine;
                    inputParametrs += "Parametrs:" + Environment.NewLine;
                    foreach (SqlParameter item in com.Parameters)
                    {
                        inputParametrs += string.Concat(item.ParameterName, ":", item.Value, " Direction:", item.Direction);
                        inputParametrs += Environment.NewLine;

                    }

                }
                Trace.Fail(string.Format(CultureInfo.InvariantCulture, FormatError, DateTime.Now, inputParametrs, ex));
            }
            finally
            {
                if (Connection.State != ConnectionState.Closed)
                    Connection.Close();
            }
            return result;
        }

        protected virtual void Dispose(bool disposed)
        {
            if (disposed)
            {
                dt.Dispose();
                dt = null;
                Connection.Close();
                Connection.Dispose();
                Connection = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
