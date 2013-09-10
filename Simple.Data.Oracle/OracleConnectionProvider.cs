using System.ComponentModel.Composition;
using System.Data;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;
#if DEVART
using Devart.Data.Oracle;
#else
using Oracle.DataAccess.Client;
#endif

namespace Simple.Data.Oracle
{
    #if DEVART 
    [Export("Devart.Data.Oracle", typeof(IConnectionProvider))]
    #else
    [Export("Oracle.DataAccess.Client", typeof(IConnectionProvider))]
    #endif
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
            if( SchemaConfiguration == null)
                SchemaConfiguration = new DefaultSchemaConfiguration(this);

            return new OracleSchemaProvider(this, SchemaConfiguration);
        }

        [Import(AllowDefault = true)]
        public ISchemaConfiguration SchemaConfiguration { get; set; }

        public string GetIdentityFunction()
        {
            return "ROWID";
        }

        public IProcedureExecutor GetProcedureExecutor(AdoAdapter adapter, ObjectName procedureName)
        {
            procedureName = new ObjectName(procedureName.Schema ?? UserOfConnection.ToUpperInvariant(), procedureName.Name);
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
            get { return ConnectionString != null ? UserIdOfConnection() : null; }
        }

        private string UserIdOfConnection()
        {
            #if !DEVART
            return new OracleConnectionStringBuilder(ConnectionString).UserID.ToUpperInvariant();
            #else
            return new OracleConnectionStringBuilder(ConnectionString).UserId.ToUpperInvariant();
            #endif
        }
    }
}
