using System;
using System.Collections.Generic;
using System.Data;
using Oracle.DataAccess.Client;

namespace Simple.Data.Oracle
{
    internal static class ProviderExtensions
    {
        public static IEnumerable<T> ReaderFrom<T>(this OracleConnectionProvider provider, string sqlText, Func<OracleDataReader, T> select)
        {
            using (var cn = provider.CreateOracleConnection())
            {
                var command = cn.CreateCommand();
                command.CommandType = CommandType.Text;
                command.CommandText = sqlText;
                cn.Open();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                        yield return select(reader);
                }
            }
        }
    }
}