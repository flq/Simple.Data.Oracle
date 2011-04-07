using NUnit.Framework;
using Simple.Data.Ado.Schema;
using System.Collections.Generic;
using System.Linq;

namespace Simple.Data.Oracle.Tests
{
    internal class OracleConnectivityContext
    {
        protected dynamic _db;
        protected const string ConnectionString = "Data Source=XE;User id=hr;Password=hr";

        protected void InitDynamicDB()
        {
            _db = Database.OpenConnection(ConnectionString);
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
    }
}