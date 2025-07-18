using System.Collections.Generic;

namespace AspNet.Identity.PostgreSQL
{
    public interface IPostgresqlDatabase
    {
        void CloseConnection();
        void Dispose();
        int Execute(string commandText, Dictionary<string, object> parameters);
        string GetStrValue(string commandText, Dictionary<string, object> parameters);
        List<Dictionary<string, string>> Query(string commandText, Dictionary<string, object> parameters);
        object QueryValue(string commandText, Dictionary<string, object> parameters);
    }
}