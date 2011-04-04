using System;
using System.Collections.Generic;
using Simple.Data.Ado.Schema;
using System.Data;

namespace Simple.Data.Oracle
{
    internal class OracleSchemaProvider : ISchemaProvider
    {
        private readonly OracleConnectionProvider _connectionProvider;

        public OracleSchemaProvider(OracleConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
            //http://www.devart.com/dotconnect/oracle/docs/DataTypeMapping.html
        }

        public IEnumerable<Table> GetTables()
        {
            yield return new Table("DUAL", null, TableType.Table);

            //using (var cn = _connectionProvider.CreateOracleConnection())
            //{
            //    var command = cn.CreateCommand();
            //    command.CommandType = CommandType.Text;
            //    //TODO: Get Tables and Views + Schema
            //    command.CommandText = "ECHO DELTA";
            //    cn.Open();
            //    using (var reader = command.ExecuteReader())
            //    {
            //        while (reader.Read())
            //            yield return new Table(reader[0].ToString(), null, TableType.Table);
            //    }

            //}
        }

        public IEnumerable<Column> GetColumns(Table table)
        {
            //new Column()
            throw new NotImplementedException();
        }

        public IEnumerable<Procedure> GetStoredProcedures()
        {
            //new Procedure()
            throw new NotImplementedException();
        }

        public IEnumerable<Parameter> GetParameters(Procedure storedProcedure)
        {
            
            throw new NotImplementedException();
        }

        public Key GetPrimaryKey(Table table)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<ForeignKey> GetForeignKeys(Table table)
        {
            throw new NotImplementedException();
        }

        public string QuoteObjectName(string unquotedName)
        {
            return string.Format("\"{0}\"", unquotedName);
        }

        public string NameParameter(string baseName)
        {
            throw new NotImplementedException();
        }
    }
}