using System;
using System.Collections.Generic;
using System.Data;

namespace Simple.Data.Oracle
{
    /// <summary>
    /// Relevant information:
    /// http://download.oracle.com/docs/html/B14164_01/featOraCommand.htm#i1007424
    /// http://www.devart.com/dotconnect/oracle/docs/DataTypeMapping.html
    /// </summary>
    public static class DbTypeConverter
    {
        private static readonly Dictionary<string, DbType> _types =
            new Dictionary<string, DbType>(StringComparer.InvariantCultureIgnoreCase)
                {
                    {"VARCHAR2", DbType.String},
                    {"NUMBER", DbType.Decimal},
                    {"CHAR", DbType.StringFixedLength},
                    {"DATE", DbType.Date},
                    {"TIMESTAMP(6)", DbType.Date},
                };

        public static DbType FromDataType(string dataType)
        {
            DbType dbType;
            var success = _types.TryGetValue(dataType, out dbType);
            return success ? dbType : DbType.Object;
        }
    }
}