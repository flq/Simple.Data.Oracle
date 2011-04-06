using System;
using System.Collections.Generic;
using Simple.Data.Ado.Schema;
using System.Data;
using System.Linq;

namespace Simple.Data.Oracle
{
    internal class OracleSchemaProvider : ISchemaProvider
    {
        private readonly OracleConnectionProvider _connectionProvider;
        private readonly SqlReflection _sqlReflection;

        public OracleSchemaProvider(OracleConnectionProvider connectionProvider)
        {
            _connectionProvider = connectionProvider;
            _sqlReflection = new SqlReflection(connectionProvider);
            //http://www.devart.com/dotconnect/oracle/docs/DataTypeMapping.html
        }

        public IEnumerable<Table> GetTables()
        {
            return _sqlReflection.UserTables().AsEnumerable();

        }

        public IEnumerable<Column> GetColumns(Table table)
        {
            return _sqlReflection.Columns(table);

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
            return new Key(
                _sqlReflection.PrimaryKeys.Where(t => t.Item1.InvariantEquals(table.ActualName))
                    .Select(t => t.Item2));
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