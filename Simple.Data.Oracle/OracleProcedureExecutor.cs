using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Linq;
using Devart.Data.Oracle;
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
            _schema = DatabaseSchema.Get(_connectionProvider, new ProviderHelper());
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
                // Double-underscore is used to denote a package name
                command.CommandText = ResolvePackageCallAndQuote(procedure);
                command.CommandType = CommandType.StoredProcedure;
                SetParameters(procedure, command, suppliedParameters);
                try
                {
                    var result = _executeImpl(command);
                    suppliedParameters["__ReturnValue"] = command.GetReturnValue();
                    RetrieveOutputParameters(command.Parameters, suppliedParameters);
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
                if (reader.FieldCount > 0)
                {
                    return reader.ToMultipleDictionaries();
                }

                // Don't call ExecuteReader for this function again.
                _executeImpl = ExecuteNonQuery;
                return Enumerable.Empty<ResultSet>();
            }
        }

        private string ResolvePackageCallAndQuote(Procedure procedure)
        {
            var parts = procedure.Name.Split(new [] {"__"}, StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 1)
                return _schema.QuoteObjectName(parts[0]);
            if (parts.Length == 2)
                return parts[0] + "." + parts[1];

            throw new InvalidOperationException("Strange state of application around getting the right procedure name");
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

            foreach (var parameter in procedure.Parameters)
            {
                if (parameter.IsReturnOrOutput())
                {
                    var p = cmd.CreateParameter();
                    p.ConfigureOutputParameterFromArgument(parameter);
                    cmd.Parameters.Add(p);
                }
                else
                {
                    object value;
                    suppliedParameters.TryGetValue("_" + i, out value);
                    cmd.Parameters.Add(parameter.Name, value);
                    i++;
                }
            }

        }

        private static void RetrieveOutputParameters(OracleParameterCollection parameters, IDictionary<string, object> suppliedParameters)
        {
            var output = from p in parameters.OfType<OracleParameter>()
                         where p.Direction == ParameterDirection.Output
                         select new {p.ParameterName, p.Value};
            foreach (var o in output)
                suppliedParameters[o.ParameterName] = o.Value;
        }

    }
}