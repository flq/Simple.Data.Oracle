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
                    {"CHAR", DbType.StringFixedLength},
                    {"NCHAR", DbType.StringFixedLength},
                    {"VARCHAR2", DbType.String},
                    {"NVARCHAR2", DbType.String},
                    {"NUMBER", DbType.Decimal},
                    {"DATE", DbType.Date},
                    {"TIMESTAMP(6)", DbType.Date},
                    {"RAW", DbType.Binary},
                    {"RAW(16)", DbType.Guid},
                };

        private static readonly Dictionary<string, Type> _dbToClr =
            new Dictionary<string, Type>
                {
                    {"CHAR", typeof (string)},
                    {"NCHAR", typeof (string)},
                    {"VARCHAR2", typeof (string)},
                    {"NVARCHAR2", typeof (string)},
                    {"NUMBER", typeof (decimal)},
                    {"DATE", typeof (DateTime)},
                    {"TIMESTAMP", typeof (DateTime)},
                    {"RAW", typeof (Guid)},
                    {"BLOB", typeof (byte[])},
                    {"REF CURSOR", typeof (object)},
                    {"PL/SQL BOOLEAN", typeof (object)},
                };

        public static DbType FromDataType(string dataType)
        {
            DbType dbType;
            var success = _types.TryGetValue(dataType, out dbType);
            return success ? dbType : DbType.Object;
        }

        public static Type ToClrType(this string oracleType)
        {
            Type type;
            var success = _dbToClr.TryGetValue(oracleType.ToUpperInvariant(), out type);
            if (!success)
                throw new ArgumentException("Oracle type " + oracleType + " could not be mapped to clr type.");
            return type;
        }
    }
}