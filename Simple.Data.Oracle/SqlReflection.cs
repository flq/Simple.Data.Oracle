using System;
using System.Collections.Generic;
using System.Configuration;
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
        private List<Tuple<string,string,DbType,int>> _columnsFlat;
        private List<Tuple<string, string>> _pks;
        private IEnumerable<ForeignKey> _fks;
        private List<Procedure> _procs;
        private List<Tuple<string, string, Type, ParameterDirection, string>> _args;

        public SqlReflection(OracleConnectionProvider provider)
        {
            _provider = provider;
            _schema = ConfigurationManager.AppSettings.AllKeys.Contains("Simple.Data.Oracle.Schema") ? ConfigurationManager.AppSettings["Simple.Data.Oracle.Schema"] : provider.UserOfConnection;
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

        public IEnumerable<Tuple<string,string,DbType,int>> Columns
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

        public List<Tuple<string, string, Type, ParameterDirection, string>> ProcedureArguments
        {
            get
            {
                _buildData.Wait();
                return _args;
            }
        }

        public string Schema
        {
            get { return _schema; }
        }

        private void BuildData()
        {
            CreateTables();
            CreateColumns();
            CreatePrimaryKeys();
            CreateForeignKeys();
            try
            {
                CreateProcedures();
                CreateProcedureArguments();
            }
            catch
            {
                // we are not currently interested in unsupported parameter errors while loading procedures
            }
        }

        private void CreatePrimaryKeys()
        {
            _pks = _provider.ReaderFrom(SqlLoader.PrimaryKeys,
                                        cmd => cmd.Parameters.Add("1", Schema.ToUpperInvariant()), 
                                        r => Tuple.Create(r.GetString(0), r.GetString(1)))
                .ToList();
        }

        private void CreateColumns()
        {
            _columnsFlat = _provider.ReaderFrom(SqlLoader.UserColumns, 
                r => Tuple.Create(
                    r.GetString(0), 
                    r.GetString(1), 
                    DbTypeConverter.FromDataType(r.GetString(2)),
                    Convert.ToInt32(r.GetDecimal(3))))
                .ToList();
            if (!Schema.Equals(_provider.UserOfConnection, StringComparison.InvariantCultureIgnoreCase))
                _columnsFlat.AddRange(_provider.ReaderFrom(SqlLoader.SchemaColumns,
                                    c =>
                                    {
                                        c.Parameters.Add("1", Schema.ToUpperInvariant());
                                        c.Parameters.Add("2", _provider.UserOfConnection.ToUpperInvariant());
                                        c.Parameters.Add("3", _provider.UserOfConnection.ToUpperInvariant());
                                        c.Parameters.Add("4", Schema.ToUpperInvariant());
                                    },
                                    r => Tuple.Create(r.GetString(0), r.GetString(1), DbTypeConverter.FromDataType(r.GetString(2)), Convert.ToInt32(r.GetDecimal(3)))).ToList());
        }

        private void CreateTables()
        {
            _tables = _provider.ReaderFrom(SqlLoader.UserTablesAndViews, r => new Table(r.GetString(0), Schema, r.GetString(1).TypeFromData()))
                .ToList();
            if (!Schema.Equals(_provider.UserOfConnection, StringComparison.InvariantCultureIgnoreCase))
                _tables.AddRange(_provider.ReaderFrom(SqlLoader.TableAccessForSchema, c =>
                {
                    c.Parameters.Add("1", _provider.UserOfConnection.ToUpperInvariant());
                    c.Parameters.Add("2", _provider.UserOfConnection.ToUpperInvariant());
                    c.Parameters.Add("3", Schema.ToUpperInvariant());
                },
                r => new Table(r.GetString(0), Schema, TableType.Table)).ToList());
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
            return new ObjectName(Schema, tableName);
        }

        private void CreateProcedures()
        {
            var procedures = _provider.ReaderFrom(SqlLoader.Procedures, 
                r => r.GetString(0) + (r.IsDBNull(1) ? "" : "__" + r.GetString(1)));

            _procs = (from p in procedures select new Procedure(p, p, _provider.UserOfConnection.ToUpperInvariant())).ToList();

            if (!Schema.Equals(_provider.UserOfConnection, StringComparison.InvariantCultureIgnoreCase))
            {
                procedures = _provider.ReaderFrom(SqlLoader.SchemaProcedures,
                                        c => c.Parameters.Add("1", Schema.ToUpperInvariant()),
                                        r => r.GetString(0) + (r.IsDBNull(1) ? "" : "__" + r.GetString(1)));
                _procs.AddRange((from p in procedures select new Procedure(p, p, Schema.ToUpperInvariant())));
            }
        }

        private class ArgDetails 
        {
            public string Owner { get; set; }
            public string ObjectName { get; set; }
            public string ArgumentName { get; set; }
            public string DataType { get; set; }
            public string Direction { get; set; }
        }

        private void CreateProcedureArguments()
        {
            var args = _provider.ReaderFrom(SqlLoader.ProcedureArguments,
                                            c => c.Parameters.Add("1", _provider.UserOfConnection.ToUpperInvariant()),
                                            r => new ArgDetails
                                                     {
                                                         Owner = _provider.UserOfConnection.ToUpperInvariant(),
                                                         ObjectName = (r.IsDBNull(1) ? "" : r.GetString(1) + "__") + r.GetString(0),
                                                         ArgumentName = r.IsDBNull(2) ? null : r.GetString(2),
                                                         DataType = r.IsDBNull(3) ? null : r.GetString(3),
                                                         Direction = r.GetString(4)
                                                     }).ToList();

            if (!Schema.Equals(_provider.UserOfConnection, StringComparison.InvariantCultureIgnoreCase))
            {
                args.AddRange(_provider.ReaderFrom(SqlLoader.ProcedureArguments,
                                                      c => c.Parameters.Add("1", Schema.ToUpperInvariant()),
                                                      r => new ArgDetails
                                                          {
                                                              Owner = Schema.ToUpperInvariant(),
                                                              ObjectName =
                                                                  (r.IsDBNull(1) ? "" : r.GetString(1) + "__") +
                                                                  r.GetString(0),
                                                              ArgumentName = r.IsDBNull(2) ? null : r.GetString(2),
                                                              DataType = r.IsDBNull(3) ? null : r.GetString(3),
                                                              Direction = r.GetString(4)
                                                          }));
            }
            // For return values, argument name is null
            // try to load as much as we can so we can support as much as possible
            _args = new List<Tuple<string, string, Type, ParameterDirection, string>>();
            foreach (var arg in args.Where(a => a.DataType != null))
            {
                try
                {
                    var type = arg.DataType.ToClrType();
                    var direction = arg.Direction.ToParameterDirection(arg.ArgumentName == null);
                    _args.Add(new Tuple<string, string, Type, ParameterDirection, string>(arg.ObjectName, arg.ArgumentName ?? "__ReturnValue", type, direction, arg.Owner));
                }
                catch
                {
                }
            }
        }
    }
}