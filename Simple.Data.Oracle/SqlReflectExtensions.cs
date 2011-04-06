using System;
using System.Collections.Generic;
using System.Data;
using Oracle.DataAccess.Client;

namespace Simple.Data.Oracle
{
    internal static class SqlReflectExtensions
    {
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