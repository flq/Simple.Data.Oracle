using System;
using System.ComponentModel.Composition;
using System.Data;
using Oracle.DataAccess.Client;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Oracle
{
    [Export("Oracle.DataAccess.Client", typeof(IConnectionProvider))]
    internal class OracleConnectionProvider : IConnectionProvider
    {

        public void SetConnectionString(string connectionString)
        {
            ConnectionString = connectionString;
        }

        IDbConnection IConnectionProvider.CreateConnection()
        {
            return CreateOracleConnection();
        }

        public ISchemaProvider GetSchemaProvider()
        {
            return new OracleSchemaProvider(this);
        }

        public string GetIdentityFunction()
        {
            return "ROWID";
        }

        public IProcedureExecutor GetProcedureExecutor(AdoAdapter adapter, ObjectName procedureName)
        {
            procedureName = new ObjectName(UserOfConnection.ToUpperInvariant(), procedureName.Name);
            return new OracleProcedureExecutor(this, procedureName);
        }

        public string ConnectionString { get; private set; }

        bool IConnectionProvider.SupportsCompoundStatements
        {
            get { return false; }
        }

        bool IConnectionProvider.SupportsStoredProcedures
        {
            get { return true; }
        }

        internal OracleConnection CreateOracleConnection()
        {
            return new OracleConnection(ConnectionString);
        }

        internal string UserOfConnection
        {
            get { return ConnectionString != null ? new OracleConnectionStringBuilder(ConnectionString).UserID.ToUpperInvariant() : null; }
        }
    }
}
