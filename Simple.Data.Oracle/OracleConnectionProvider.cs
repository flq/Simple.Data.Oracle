using System;
using System.ComponentModel.Composition;
using System.Data;
using Oracle.DataAccess.Client;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Oracle
{
    [Export("sql", typeof(IConnectionProvider))]
    internal class OracleConnectionProvider : IConnectionProvider
    {

        public void SetConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            return CreateOracleConnection();
        }

        public ISchemaProvider GetSchemaProvider()
        {
            return new OracleSchemaProvider(this);
        }

        public string GetIdentityFunction()
        {
            throw new NotSupportedException("Currently unclear what this is used for and what's the best way to support in Oracle");
        }

        public IProcedureExecutor GetProcedureExecutor(AdoAdapter adapter, ObjectName procedureName)
        {
            throw new NotImplementedException();
        }

        public string ConnectionString { get; private set; }

        public bool SupportsCompoundStatements
        {
            get { return false; }
        }

        public bool SupportsStoredProcedures
        {
            get { return true; }
        }

        internal OracleConnection CreateOracleConnection()
        {
            return new OracleConnection(ConnectionString);
        }
    }
}
