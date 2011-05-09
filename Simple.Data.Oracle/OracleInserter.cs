using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Data;
using System.Linq;
using System.Text;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;
using Simple.Data.Extensions;

namespace Simple.Data.Oracle
{
    [Export(typeof(ICustomInserter))]
    public class OracleInserter : ICustomInserter
    {
        public IDictionary<string, object> Insert(AdoAdapter adapter, string tableName, IDictionary<string, object> data)
        {
            var s = DatabaseSchema.Get(adapter.ConnectionProvider);
            var table = s.FindTable(tableName);
            
            var tuples = InitializeInsertion(table);
            foreach (var d in data)
                tuples[d.Key.Homogenize()].InsertedValue = d.Value;

            IDbCommand cmd = null;
            using (cmd = ConstructCommand(tuples, table.QualifiedName, ()=>adapter.ConnectionProvider.CreateConnection().CreateCommand()))
            {
                cmd.Connection.Open();
                cmd.ExecuteNonQuery();
            }

            return data;
        }

        private IDbCommand ConstructCommand(IDictionary<string, InsertTuple> tuples, string qualifiedTableName, Func<IDbCommand> createCommand)
        {
            var colsSB = new StringBuilder();
            var valsSB = new StringBuilder();

            var cmd = createCommand();
            foreach (var t in tuples.Values.Where(it=>it.ToBeInserted))
            {
                colsSB.Append(", ");
                colsSB.Append(t.QuotedDbColumn);
                valsSB.Append(", ");
                valsSB.Append(t.InsertionParameterName);
                var p = cmd.CreateParameter();
                p.ParameterName = t.InsertionParameterName;
                p.Value = t.InsertedValue;
                cmd.Parameters.Add(p);
            }

            var sql = string.Format("INSERT INTO {0} ({1}) VALUES ({2})", qualifiedTableName, colsSB.ToString().Substring(2),
                                    valsSB.ToString().Substring(2));


            cmd.CommandText = sql;
            return cmd;
        }

        private static IDictionary<string,InsertTuple> InitializeInsertion(Table table)
        {
            return table.Columns
                .Select((c, i) => new InsertTuple
                                      {
                                          Column = c,
                                          QuotedDbColumn = c.QuotedName,
                                          SimpleDataColumn = c.HomogenizedName,
                                          InsertionParameterName = ":pi" + i,
                                          ReturningParameterName = ":ri" + i
                                      })
                .ToDictionary(it => it.SimpleDataColumn, it => it);
        }

        private class InsertTuple
        {
            public string SimpleDataColumn;
            public string QuotedDbColumn;
            public string InsertionParameterName;
            public string ReturningParameterName;
            public Column Column;
            public object ReturnedValue;

            private object _insertedValue;
            public object InsertedValue
            {
                get { return _insertedValue; }
                set
                {
                    ToBeInserted = true;
                    _insertedValue = value;
                }
            }

            public bool ToBeInserted { get; private set; }
        }
    }
}