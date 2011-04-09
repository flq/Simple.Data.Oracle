using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using Oracle.DataAccess.Client;
using Simple.Data.Ado;
using Simple.Data.Ado.Schema;
using ResultSet = System.Collections.Generic.IEnumerable<System.Collections.Generic.IDictionary<string, object>>;

namespace Simple.Data.Oracle
{
    internal class OracleProcedureExecutor : IProcedureExecutor
    {
        private readonly OracleConnectionProvider _connectionProvider;
        private readonly ObjectName _procedureName;
        private readonly DatabaseSchema _schema;
        private Func<IDbCommand, IEnumerable<ResultSet>> _executeImpl;

        public OracleProcedureExecutor(OracleConnectionProvider connectionProvider, ObjectName procedureName)
        {
            _connectionProvider = connectionProvider;
            _schema = DatabaseSchema.Get(_connectionProvider);
            _procedureName = procedureName;
            _executeImpl = ExecuteReader;
        }

        public IEnumerable<ResultSet> Execute(IDictionary<string, object> suppliedParameters)
        {
            var procedure = _schema.FindProcedure(_procedureName);
            if (procedure == null)
            {
                throw new UnresolvableObjectException(_procedureName.ToString());
            }

            using (var cn = _connectionProvider.CreateOracleConnection())
            using (var command = cn.CreateCommand())
            {
                command.CommandText = _schema.QuoteObjectName(procedure.Name);
                command.CommandType = CommandType.StoredProcedure;
                SetParameters(procedure, command, suppliedParameters);
                try
                {
                    var result = _executeImpl(command);
                    //TODO: Handle Output values
                    //RetrieveOutputParameterValues(procedure, command, suppliedParameters);
                    return result;
                }
                catch (DbException ex)
                {
                    throw new AdoAdapterException(ex.Message, command);
                }
            }
        }

        public IEnumerable<ResultSet> ExecuteReader(IDbCommand command)
        {
            command.WriteTrace();
            command.Connection.Open();

            using (var reader = command.ExecuteReader())
            {
                //TODO: ExecuteReader won't cut it for a proc with return val, need to extract the return value parameter
                if (reader.FieldCount > 0)
                {
                    return reader.ToMultipleDictionaries();
                }

                // Don't call ExecuteReader for this function again.
                _executeImpl = ExecuteNonQuery;
                return Enumerable.Empty<ResultSet>();
            }
        }

        private static IEnumerable<ResultSet> ExecuteNonQuery(IDbCommand command)
        {
            command.WriteTrace();
            Trace.TraceInformation("ExecuteNonQuery", "Simple.Data.SqlTest");
            command.Connection.Open();
            command.ExecuteNonQuery();
            return Enumerable.Empty<ResultSet>();
        }

        private static void SetParameters(Procedure procedure, OracleCommand cmd, IDictionary<string, object> suppliedParameters)
        {
            int i = 0;

            if (procedure.HasReturnValue())
            {
                cmd.Parameters.Add(CreateReturnParameter(cmd.CreateParameter(), procedure.ReturnValueDefault()));
            }

            foreach (var parameter in procedure.InputParameters())
            {
                object value;
                if (!suppliedParameters.TryGetValue(parameter.Name.Replace("@", ""), out value))
                {
                    suppliedParameters.TryGetValue("_" + i, out value);
                }
                cmd.Parameters.Add(parameter.Name, value);
                i++;
            }
        }

        /// <summary>
        /// http://forums.asp.net/t/791115.aspx/1?ODP+NET+Function+call+with+VARCHAR2+return+value+cause+ERROR
        /// It causes an error to keep a the return value parameter with value null. Hence we pass in a 
        /// value that corresponds to the type of the return parameter.
        /// </summary>
        private static OracleParameter CreateReturnParameter(OracleParameter returnParam, object defaultValue)
        {
            returnParam.ParameterName = "__RETVAL";
            returnParam.Direction = ParameterDirection.ReturnValue;
            returnParam.Value = defaultValue;
            return returnParam;

        }
    }
}