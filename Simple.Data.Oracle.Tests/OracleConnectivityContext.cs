using System.Configuration;
using Devart.Data.Oracle;
using Simple.Data.Ado.Schema;
using System.Collections.Generic;
using System.Linq;

namespace Simple.Data.Oracle.Tests
{
    internal class OracleConnectivityContext
    {
        protected dynamic _db;
        protected string _connectionString;
        protected string _providerName;

        public OracleConnectivityContext()
        {
            string connectionName = "DevartOracle";
            _connectionString = ConfigurationManager.ConnectionStrings[connectionName].ConnectionString;
            _providerName = ConfigurationManager.ConnectionStrings[connectionName].ProviderName;
        }

        protected void InitDynamicDB()
        {
            _db = Database.Opener.OpenConnection(_connectionString, _providerName);
        }

        protected List<Table> Tables { get; private set; }

        protected Table TableByName(string name)
        {
            return Tables.Single(t => t.ActualName.InvariantEquals(name));
        }

        protected OracleConnectionProvider GetConnectionProvider()
        {
            var p = new OracleConnectionProvider();
            p.SetConnectionString(_connectionString);
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
            var con = new OracleConnection(_connectionString);
            var c = new OracleCommand(text, con);
            return c;
        }
    }
}