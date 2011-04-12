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
        
        /// <summary>
        /// http://forums.asp.net/t/791115.aspx/1?ODP+NET+Function+call+with+VARCHAR2+return+value+cause+ERROR
        /// It causes an error to keep a the return value parameter with value null. Hence we pass in a 
        /// value that corresponds to the type of the return parameter. Another Hack is to set the size of
        /// a varchar return value, otherwise it is 0 which cannot contain a lot of characters.
        /// </summary>
        public static void ConfigureOutputParameterFromArgument(this OracleParameter parameter, Parameter definition)
        {
            parameter.ParameterName = definition.Name;
            parameter.Direction = definition.Direction;
            if (definition.Type == typeof(string))
            {
                parameter.Value = "A";
                parameter.Size = 4000;
            }
            if (definition.Type == typeof(decimal))
                parameter.Value = 10m;
            if (definition.Type == typeof(DateTime))
                parameter.Value = DateTime.MinValue;
        }


        public static TableType TypeFromData(this string type)
        {
            if (type.Equals("view", StringComparison.InvariantCultureIgnoreCase))
                return TableType.View;
            if (type.Equals("table", StringComparison.InvariantCultureIgnoreCase))
                return TableType.Table;
            throw new InvalidOperationException("Bad type provided: " + type);
        }

        public static object GetReturnValue(this OracleCommand cmd)
        {
            var param = cmd.Parameters.OfType<OracleParameter>().FirstOrDefault(p => p.Direction == ParameterDirection.ReturnValue);
            return param != null ? param.Value : null;
        }

        public static bool IsReturnOrOutput(this Parameter p)
        {
            return p.Direction == ParameterDirection.Output || p.Direction == ParameterDirection.ReturnValue;
        }

        public static IEnumerable<Parameter> InputParameters(this Procedure proc)
        {
            return proc.Parameters.Where(p => p.Direction == ParameterDirection.Input);
        }

        public static IEnumerable<Parameter> OutputParameters(this Procedure proc)
        {
            return proc.Parameters.Where(p => p.Direction == ParameterDirection.Output || p.Direction == ParameterDirection.ReturnValue);
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