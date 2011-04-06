using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Simple.Data.Ado.Schema;
using Simple.Data.Oracle.ReflectionSql;
using System.Linq;

namespace Simple.Data.Oracle
{
    internal class SqlReflection
    {
        private readonly OracleConnectionProvider _provider;
        private readonly string _schema;
        private readonly Task _buildData;
        
        private List<Table> _tables;
        private List<Tuple<string,string>> _columnsFlat;
        private List<Tuple<string, string>> _pks;

        public SqlReflection(OracleConnectionProvider provider)
        {
            _provider = provider;
            _schema = provider.UserOfConnection;
            _buildData = new Task(BuildData);
            _buildData.Start();
        }

        public List<Tuple<string, string>> PrimaryKeys
        {
            get
            {
                _buildData.Wait();
                return _pks;
            }
        }

        private void BuildData()
        {
            _tables = _provider.ReaderFrom(SqlLoader.UserTablesAndViews, r => new Table(r[0].ToString(), _schema, TypeFromData(r[1].ToString())))
                            .ToList();
            _tables.Add(new Table("DUAL", null, TableType.Table));

            _columnsFlat = _provider.ReaderFrom(SqlLoader.UserColumns, r => Tuple.Create(r[0].ToString(), r[1].ToString())).ToList();

            _pks = _provider.ReaderFrom(SqlLoader.PrimaryKeys,
                                        cmd => cmd.Parameters.Add("1", _schema.ToUpperInvariant()), 
                                        r => Tuple.Create(r.GetString(0), r.GetString(1)))
                                        .ToList();
        }

        public IEnumerable<Table> UserTables()
        {
            _buildData.Wait();
            return _tables;
        }

        public IEnumerable<Column> Columns(Table table)
        {
            _buildData.Wait();
            return _columnsFlat
                .Where(c => table.ActualName.Equals(c.Item1, StringComparison.InvariantCultureIgnoreCase))
                .Select(c => new Column(c.Item2, table));
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