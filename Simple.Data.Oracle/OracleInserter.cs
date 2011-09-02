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
        public IDictionary<string, object> Insert(AdoAdapter adapter, string tableName, IDictionary<string, object> data, IDbTransaction transaction)
        {
            var s = DatabaseSchema.Get(adapter.ConnectionProvider, new ProviderHelper());
            var table = s.FindTable(tableName);
            
            var tuples = InitializeInsertion(table);
            foreach (var d in data)
                tuples[d.Key.Homogenize()].InsertedValue = d.Value;

            Func<IDbCommand> command =
                () =>
                    {
                        var c = transaction != null
                                    ? transaction.Connection.CreateCommand()
                                    : adapter.ConnectionProvider.CreateConnection().CreateCommand();
                        return c;
                    };

            IDbCommand cmd;
            using (cmd = ConstructCommand(tuples, table.QualifiedName, command))
            {
                cmd.WriteTrace();
                cmd.Connection.TryOpen();
                cmd.ExecuteNonQuery();
                var returnData = new DbDictionary();
                foreach (var it in tuples.Values)
                    returnData.Add(it.SimpleDataColumn, NormalizeReturningValue((IDbDataParameter)cmd.Parameters[it.ReturningParameterName]));
                data = returnData;
            }

            return data;
        }

        private IDbCommand ConstructCommand(IDictionary<string, InsertTuple> tuples, string qualifiedTableName, Func<IDbCommand> createCommand)
        {
            var cmd = createCommand();

            var insertPart = GetInsertPart(tuples, cmd, qualifiedTableName);
            var returningPart = GetReturningPart(tuples, cmd);

            cmd.CommandText = string.Format("{0} {1}", insertPart, returningPart);
            return cmd;
        }

        private string GetInsertPart(IDictionary<string, InsertTuple> tuples, IDbCommand cmd, string qualifiedTableName)
        {
            var colsSB = new StringBuilder();
            var valsSB = new StringBuilder();
            
            foreach (var t in tuples.Values.Where(it=>it.ToBeInserted))
            {
                colsSB.Append(", ");
                colsSB.Append(t.QuotedDbColumn);
                valsSB.Append(", ");
                valsSB.Append(t.InsertionSlotText);
                if (!t.RequiresInputParameterDefinition)
                    continue;
                var p = cmd.CreateParameter();
                p.ParameterName = t.InsertionSlotText;
                p.Value = NormalizeInsertedValue(t);
                cmd.Parameters.Add(p);
            }

            return string.Format("INSERT INTO {0} ({1}) VALUES ({2})", qualifiedTableName, colsSB.ToString().Substring(2),
                                 valsSB.ToString().Substring(2));
        }

        private static object NormalizeInsertedValue(InsertTuple t)
        {
            var value = t.InsertedValue;
            if (value == null)
                return DBNull.Value;
            if (value is Guid)
                return ((Guid) t.InsertedValue).ToByteArray();
            return t.InsertedValue;
        }

        private static object NormalizeReturningValue(IDbDataParameter dbDataParameter)
        {
            if (dbDataParameter.DbType == DbType.Binary && dbDataParameter.Size == 16)
                return new Guid((byte[])dbDataParameter.Value);
            return dbDataParameter.Value;
        }

        private string GetReturningPart(IDictionary<string, InsertTuple> tuples, IDbCommand cmd)
        {
            var colsSB = new StringBuilder();
            var parsSB = new StringBuilder();

            foreach (var t in tuples.Values)
            {
                colsSB.Append(", ");
                colsSB.Append(t.QuotedDbColumn);
                parsSB.Append(", ");
                parsSB.Append((t.ReturningParameterName));
                var p = cmd.CreateParameter();
                p.ParameterName = t.ReturningParameterName;
                p.DbType = t.Column.DbType;
                p.Direction = ParameterDirection.Output;
                p.Size = t.Column.MaxLength;
                cmd.Parameters.Add(p);
            }

            return string.Format("RETURNING {0} INTO {1}", colsSB.ToString().Substring(2),
                                 parsSB.ToString().Substring(2));
        }

        private static IDictionary<string,InsertTuple> InitializeInsertion(Table table)
        {
            return table.Columns
                .Select((c, i) => new InsertTuple
                                      {
                                          Column = c,
                                          QuotedDbColumn = c.QuotedName,
                                          SimpleDataColumn = c.HomogenizedName,
                                          InsertionSlotText = ":pi" + i,
                                          ReturningParameterName = ":ri" + i
                                      })
                .ToDictionary(it => it.SimpleDataColumn, it => it);
        }

        private class InsertTuple
        {
            public string SimpleDataColumn;
            public string QuotedDbColumn;
            public string InsertionSlotText;
            public bool RequiresInputParameterDefinition = true;
            public string ReturningParameterName;
            public Column Column;

            private object _insertedValue;
            public object InsertedValue
            {
                get { return _insertedValue; }
                set
                {
                    ToBeInserted = true;
                    _insertedValue = value;
                    if (!(value is Sequence)) 
                        return;
                    
                    InsertionSlotText = value.ToString();
                    RequiresInputParameterDefinition = false;
                }
            }

            public bool ToBeInserted { get; private set; }
        }
    }
}