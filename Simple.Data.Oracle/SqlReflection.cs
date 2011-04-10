using System;
using System.Collections.Generic;
using System.Data;
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
        private List<Procedure> _procs;
        private List<Tuple<string, string, Type, ParameterDirection>> _args;

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

        public IEnumerable<Procedure> Procedures
        {
            get
            {
                _buildData.Wait();
                return _procs;
            }
        }

        public List<Tuple<string, string, Type, ParameterDirection>> ProcedureArguments
        {
            get
            {
                _buildData.Wait();
                return _args;
            }
        }

        private void BuildData()
        {
            CreateTables();
            CreateColumns();
            CreatePrimaryKeys();
            CreateForeignKeys();
            CreateProcedures();
            CreateProcedureArguments();
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
            _columnsFlat = _provider.ReaderFrom(SqlLoader.UserColumns, r => Tuple.Create(r.GetString(0), r.GetString(1))).ToList();
        }

        private void CreateTables()
        {
            _tables = _provider.ReaderFrom(SqlLoader.UserTablesAndViews, r => new Table(r.GetString(0), _schema, r.GetString(1).TypeFromData()))
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

            _fks = (from fk in foreignKeys
                         group fk by new {fk.FkTableName, fk.PkTableName}
                         into tableGrouping
                         let fks = tableGrouping.Select(z => z.FkColumnName)
                         let pks = tableGrouping.Select(z => z.PkColumnName)
                         select
                             new ForeignKey(ObjectNameFrom(tableGrouping.Key.FkTableName), fks,
                                            ObjectNameFrom(tableGrouping.Key.PkTableName), pks)).ToList();
        }

        private ObjectName ObjectNameFrom(string tableName)
        {
            return new ObjectName(_schema, tableName);
        }

        private void CreateProcedures()
        {
            var procedures = _provider.ReaderFrom(SqlLoader.Procedures, 
                r => r.GetString(0) + (r.IsDBNull(1) ? "" : "__" + r.GetString(1)));

            _procs = (from p in procedures select new Procedure(p, p, _schema)).ToList();
        }

        private void CreateProcedureArguments()
        {
            var args = _provider.ReaderFrom(SqlLoader.ProcedureArguments,
                                            c => c.Parameters.Add("1", _schema.ToUpperInvariant()),
                                            r => new
                                                     {
                                                         ObjectName = (r.IsDBNull(1) ? "" : r.GetString(1) + "__") + r.GetString(0),
                                                         ArgumentName = r.IsDBNull(2) ? null : r.GetString(2),
                                                         DataType = r.GetString(3),
                                                         Direction = r.GetString(4)
                                                     });
            
            // For return values, argument name is null
            _args = (from a in args
                    let type = a.DataType.ToClrType()
                    let direction = a.Direction.ToParameterDirection(a.ArgumentName == null)
                    select Tuple.Create(a.ObjectName, a.ArgumentName ?? "__ReturnValue",  type, direction)).ToList();

        }
    }
}