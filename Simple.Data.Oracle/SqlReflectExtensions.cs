using System;
using System.Collections.Generic;
using System.Data;
using Oracle.DataAccess.Client;
using Simple.Data.Ado.Schema;
using System.Linq;

namespace Simple.Data.Oracle
{
    internal static class SqlReflectExtensions
    {
        private static Dictionary<string, Type> _dbToClr = new Dictionary<string, Type>
                                                               {
                                                                   { "VARCHAR2", typeof(string) },
                                                                   { "NUMBER", typeof(decimal) },
                                                                   { "DATE", typeof(DateTime) }
                                                               };

        public static object ReturnValueDefault(this Procedure proc)
        {
            var retArg = proc.Parameters.First(p => p.Direction == ParameterDirection.ReturnValue);
            if (retArg.Type == typeof (string))
                return "A";
            if (retArg.Type == typeof(decimal))
                return 10m;
            if (retArg.Type == typeof(DateTime))
                return DateTime.MinValue;
            throw new ArgumentException("Lost in assigning a default value for return value type " + retArg.Type.Name);
        }

        public static TableType TypeFromData(this string type)
        {
            if (type.Equals("view", StringComparison.InvariantCultureIgnoreCase))
                return TableType.View;
            if (type.Equals("table", StringComparison.InvariantCultureIgnoreCase))
                return TableType.Table;
            throw new InvalidOperationException("Bad type provided: " + type);
        }

        public static bool HasReturnValue(this Procedure proc)
        {
            return proc.Parameters.Any(p => p.Direction == ParameterDirection.ReturnValue);
        }

        public static object GetReturnValue(this OracleCommand cmd)
        {
            var param = cmd.Parameters.OfType<OracleParameter>().FirstOrDefault(p => p.Direction == ParameterDirection.ReturnValue);
            return param != null ? param.Value : null;
        }

        public static IEnumerable<Parameter> InputParameters(this Procedure proc)
        {
            return proc.Parameters.Where(p => p.Direction == ParameterDirection.Input);
        }

        public static Type ToClrType(this string oracleType)
        {
            Type type;
            var success = _dbToClr.TryGetValue(oracleType.ToUpperInvariant(), out type);
            if (!success)
                throw new ArgumentException("Oracle type " + oracleType + " could not be mapped to clr type.");
            return type;
        }

        public static ParameterDirection ToParameterDirection(this string direction, bool treatOutputAsReturn)
        {
            switch (direction.ToUpperInvariant())
            {
                case "IN": return ParameterDirection.Input;
                case "OUT": return treatOutputAsReturn ? ParameterDirection.ReturnValue : ParameterDirection.Output;
                default: throw new ArgumentException("Currently only input, output, return value supported");
            }
        }

        public static bool InvariantEquals(this string s, string other)
        {
            return s.Equals(other, StringComparison.InvariantCultureIgnoreCase);
        }

        public static IEnumerable<T> ReaderFrom<T>(this OracleConnectionProvider provider, string sqlText, Action<OracleCommand> modCommand, Func<OracleDataReader, T> select)
        {
            using (var cn = provider.CreateOracleConnection())
            {
                var command = cn.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlText;
                modCommand(command);
                cn.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        yield return select(reader);
                }
            }
        }

        public static IEnumerable<T> ReaderFrom<T>(this OracleConnectionProvider provider, string sqlText, Func<OracleDataReader, T> select)
        {
            return provider.ReaderFrom(sqlText, _ => { }, select);
        }
    }
}