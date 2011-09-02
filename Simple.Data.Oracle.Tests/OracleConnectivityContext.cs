using Oracle.DataAccess.Client;
using Simple.Data.Ado.Schema;
using System.Collections.Generic;
using System.Linq;

namespace Simple.Data.Oracle.Tests
{
    internal class OracleConnectivityContext
    {
        protected dynamic _db;
        protected const string ConnectionString = "Data Source=XE;User id=hr;Password=hr";
        protected const string ProviderName = "Oracle.DataAccess.Client";

        protected void InitDynamicDB()
        {
            _db = Database.Opener.OpenConnection(ConnectionString, "Oracle.DataAccess.Client");
        }

        protected List<Table> Tables { get; private set; }

        protected Table TableByName(string name)
        {
            return Tables.Single(t => t.ActualName.InvariantEquals(name));
        }

        protected OracleConnectionProvider GetConnectionProvider()
        {
            var p = new OracleConnectionProvider();
            p.SetConnectionString(ConnectionString);
            return p;
        }

        protected SqlReflection GetSqlReflection()
        {
            return new SqlReflection(GetConnectionProvider());
        }

        protected OracleSchemaProvider GetSchemaProvider()
        {
            var schemaProvider = new OracleSchemaProvider(GetConnectionProvider());
            Tables = schemaProvider.GetTables().ToList();
            return schemaProvider;
        }

        protected OracleCommand GetCommand(string text)
        {
            var con = new OracleConnection(ConnectionString);
            var c = new OracleCommand(text, con);
            return c;
        }
    }
}