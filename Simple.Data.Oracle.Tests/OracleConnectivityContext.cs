using NUnit.Framework;

namespace Simple.Data.Oracle.Tests
{
    internal class OracleConnectivityContext
    {
        protected dynamic _db;
        protected const string ConnectionString = "Data Source=XE;User id=hr;Password=hr";

        protected void CreateDbObject()
        {
            _db = Database.OpenConnection(ConnectionString);
        }

        protected OracleConnectionProvider ConstructProvider()
        {
            var p = new OracleConnectionProvider();
            p.SetConnectionString(ConnectionString);
            return p;
        }
    }
}