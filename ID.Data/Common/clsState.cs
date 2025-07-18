using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UserState
{
    public class clsState
    {
        private readonly Guid guid;

        public clsState(Guid g)
        {
            guid = g;
        }

        public static Guid GetNewStateId
        {
            get { return Guid.NewGuid(); }
        }

        public object GetValue(string key)
        {
            object result = null;
            using (var sqlClient = new SqlDataAccess())
            {
                var cmd = sqlClient.Command;
                cmd.CommandText = "GetStateUser";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                var parState = new SqlParameter("@StateId", System.Data.SqlDbType.VarChar, 50) { Value = guid.ToString() };
                cmd.Parameters.Add(parState);
                var parKey = new SqlParameter("@KeyDic", System.Data.SqlDbType.VarChar, 50) { Value = key };
                cmd.Parameters.Add(parKey);
                result = sqlClient.GetObjectValue(cmd);
            }
            return result;
        }

        public void UpdateData(string key, byte[] value)
        {
            using (var sqlClient = new SqlDataAccess())
            {
                var cmd = sqlClient.Command;
                cmd.CommandText = "UpdateStateUser";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                var parState = new SqlParameter("@StateId", System.Data.SqlDbType.VarChar, 50) { Value = guid.ToString() };
                cmd.Parameters.Add(parState);
                var parKey = new SqlParameter("@KeyDic", System.Data.SqlDbType.VarChar, 50) { Value = key };
                cmd.Parameters.Add(parKey);
                var parValue = new SqlParameter("@Value", System.Data.SqlDbType.VarChar, 50) { Value = value };
                cmd.Parameters.Add(parValue);
                sqlClient.ExecuteWithoutQuery(cmd);
            }
        }

        public void AddStateDesk(string userName, int companyId, string ipAdress)
        {
            using (var sqlClient = new SqlDataAccess())
            {
                var cmd = sqlClient.Command;
                cmd.CommandText = "AddStateDesk";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@StateId", guid.ToString());
                cmd.Parameters.AddWithValue("@UserName", userName);
                cmd.Parameters.AddWithValue("@IPAdress", ipAdress);
                cmd.Parameters.AddWithValue("@CompanyId", companyId);
                sqlClient.ExecuteWithoutQuery(cmd);
            }
        }

        public void Clear()
        {
            using (var sqlClient = new SqlDataAccess())
            {
                var cmd = sqlClient.Command;
                cmd.CommandText = "ClearStateDesk";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                var parState = new SqlParameter("@StateId", System.Data.SqlDbType.VarChar, 50) { Value = guid.ToString() };
                cmd.Parameters.Add(parState);
                sqlClient.ExecuteWithoutQuery(cmd);

            }
        }

        public void AddStateDesc(string userName, int parse, string ipAddress)
        {
            throw new NotImplementedException();
        }

        public void updateData(string name, byte[] getBuffer)
        {
            throw new NotImplementedException();
        }

        public object getValue(string key)
        {
            object result = null;
            using (var client = new SqlDataAccess())
            {
                var cmd = client.Command;
                cmd.CommandText = "GetStateUser";
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                var parState = new SqlParameter("@StateId", System.Data.SqlDbType.VarChar, 50) {Value = guid.ToString()};
                cmd.Parameters.Add(parState);
                var parKey = new SqlParameter("@KeyDic", System.Data.SqlDbType.VarChar, 50) {Value = key};
                cmd.Parameters.Add(parState);
                result = client.GetObjectValue(cmd);
            }
            return result;
        }
    }
}
