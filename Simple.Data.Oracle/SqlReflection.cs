using System;
using System.Collections.Generic;
using System.Data;
using Simple.Data.Ado.Schema;
using Simple.Data.Oracle.ReflectionSql;
using System.Linq;

namespace Simple.Data.Oracle
{
    internal class SqlReflection
    {
        private readonly OracleConnectionProvider _provider;
        private string _schema;

        public SqlReflection(OracleConnectionProvider provider)
        {
            _provider = provider;
            _schema = provider.UserOfConnection;
        }

        public IEnumerable<Table> UserTables()
        {
            return _provider.ReaderFrom(SqlLoader.UserTablesAndViews, r => new Table(r[0].ToString(), _schema, TypeFromData(r[1].ToString())))
                            .AsEnumerable();
        }

        private static TableType TypeFromData(string type)
        {
            if (type.Equals("view", StringComparison.InvariantCultureIgnoreCase))
                return TableType.View;
            if (type.Equals("table", StringComparison.InvariantCultureIgnoreCase))
                return TableType.Table;
            throw new InvalidOperationException("Bad type provided: " + type);
        }
    }
}