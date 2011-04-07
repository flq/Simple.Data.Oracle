using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Simple.Data.Ado;
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
        private IEnumerable<ForeignKey> _fks;

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

        public IEnumerable<Table> Tables
        {
            get
            {
                _buildData.Wait();
                return _tables;
            }
        }

        public IEnumerable<Tuple<string,string>> Columns
        {
            get
            {
                _buildData.Wait();
                return _columnsFlat;
            }
        }

        public IEnumerable<ForeignKey> ForeignKeys
        {
            get
            {
                _buildData.Wait();
                return _fks;
            }
        }

        private void BuildData()
        {
            CreateTables();
            CreateColumns();
            CreatePrimaryKeys();
            CreateForeignKeys();
        }

        private void CreatePrimaryKeys()
        {
            _pks = _provider.ReaderFrom(SqlLoader.PrimaryKeys,
                                        cmd => cmd.Parameters.Add("1", _schema.ToUpperInvariant()), 
                                        r => Tuple.Create(r.GetString(0), r.GetString(1)))
                .ToList();
        }

        private void CreateColumns()
        {
            _columnsFlat = _provider.ReaderFrom(SqlLoader.UserColumns, r => Tuple.Create(r[0].ToString(), r[1].ToString())).ToList();
        }

        private void CreateTables()
        {
            _tables = _provider.ReaderFrom(SqlLoader.UserTablesAndViews, r => new Table(r[0].ToString(), _schema, TypeFromData(r[1].ToString())))
                .ToList();
            _tables.Add(new Table("DUAL", null, TableType.Table));
        }

        private void CreateForeignKeys()
        {
            var foreignKeys = _provider.ReaderFrom(SqlLoader.ForeignKeys, 
                r => new
                         {
                             FkTableName = r.GetString(0).ToUpperInvariant(),
                             FkColumnName = r.GetString(1).ToUpperInvariant(),
                             PkTableName = r.GetString(2).ToUpperInvariant(),
                             PkColumnName = r.GetString(3).ToUpperInvariant()
                         });

            _fks = from fk in foreignKeys
                         group fk by new {fk.FkTableName, fk.PkTableName}
                         into tableGrouping
                         let fks = tableGrouping.Select(z => z.FkColumnName)
                         let pks = tableGrouping.Select(z => z.PkColumnName)
                         select
                             new ForeignKey(ObjectNameFrom(tableGrouping.Key.FkTableName), fks,
                                            ObjectNameFrom(tableGrouping.Key.PkTableName), pks);
        }

        private ObjectName ObjectNameFrom(string tableName)
        {
            return new ObjectName(_schema, tableName);
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