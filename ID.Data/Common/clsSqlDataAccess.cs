using System;
using System.Data;
using System.Data.SqlClient;

namespace UserState
{
    public class SqlDataAccess: IDisposable
    {
        private SqlConnection _connection;

        public SqlDataAccess()
        {
            this._connection = new SqlConnection(Properties.Settings.Default["StateConnectionStr"].ToString());
        }

        public SqlCommand Command
        {
            get { return _connection.CreateCommand(); }
        }

         protected virtual void Dispose(bool disposed)
        {
            if (disposed)
            {
                _connection.Close();
                _connection.Dispose();
                _connection = null;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public bool ExecuteWithoutQuery(SqlCommand cmd)
        {
            bool result;
            try
            {
                _connection.Open();
                result = cmd.ExecuteNonQuery() > 0;
            }
            catch (Exception ex)
            {
                var inputParameters = GetCommandParams(cmd);
                throw new Exception(inputParameters + " Error: " + ex);
            }
            return result;
        }

        public object GetObjectValue(SqlCommand cmd)
        {
            object result = null;

            try
            {
                _connection.Open();
                result = cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                var inputParameters = GetCommandParams(cmd);
                throw new Exception(inputParameters + " Error: " + ex);
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                    _connection.Close();
            }
            return result;
        }

        private static string GetCommandParams(SqlCommand cmd)
        {
            var inputParameters = string.Empty;
            if (cmd != null)
            {
                inputParameters = "Command Text:" + cmd.CommandText + Environment.NewLine;
                inputParameters += "Parameters:" + Environment.NewLine;
                foreach (SqlParameter item in cmd.Parameters)
                {
                    inputParameters += string.Concat(item.ParameterName, ":", item.Value, " Direction:",
                        item.Direction);
                    inputParameters += Environment.NewLine;
                }
            }
            return inputParameters;
        }
    }
}