using System;
using System.Collections.Generic;
using System.Data;

namespace Simple.Data.Oracle
{
    /// <summary>
    /// Relevant information:
    /// http://download.oracle.com/docs/html/B14164_01/featOraCommand.htm#i1007424
    /// http://www.devart.com/dotconnect/oracle/docs/DataTypeMapping.html
    /// Treat UNDEFINED as a string because XMLTYPE parameters are stored in ALL_ARGUMENTS with a DATA_TYPE of UNDEFINED
    /// </summary>
    public static class DbTypeConverter
    {
        private static readonly Dictionary<string, DbType> _dbTypes =
            new Dictionary<string, DbType>(StringComparer.InvariantCultureIgnoreCase)
                {
                    {"CHAR", DbType.StringFixedLength},
                    {"NCHAR", DbType.StringFixedLength},
                    {"VARCHAR2", DbType.String},
                    {"NVARCHAR2", DbType.String},
                    {"NUMBER", DbType.Decimal},
                    {"BLOB", DbType.Binary},
                    {"CLOB", DbType.String},
                    {"NCLOB", DbType.String},
                    {"ROWID", DbType.String},
                    {"UROWID", DbType.String},
                    {"XMLTYPE", DbType.String},
                    {"LONG", DbType.Decimal},
                    {"FLOAT", DbType.Decimal},
                    {"REAL", DbType.Decimal},
                    {"BINARY_FLOAT", DbType.Decimal},
                    {"BINARY_DOUBLE", DbType.Decimal},
                    {"DATE", DbType.Date},
                    {"TIMESTAMP(6)", DbType.Date},
                    {"TIMESTAMP", DbType.Date},
                    {"TIMESTAMP WITH TIME ZONE", DbType.Date},
                    {"TIMESTAMP WITH LOCAL TIME ZONE", DbType.Date},
                    {"INTERVAL YEAR TO MONTH", DbType.String},
                    {"INTERVAL DAY TO SECOND", DbType.Time},
                    {"RAW", DbType.Binary},
                    {"RAW(16)", DbType.Guid},
                    {"UNDEFINED", DbType.String}
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
                    {"CLOB", typeof (string)},
                    {"NCLOB", typeof (string)},
                    {"ROWID", typeof (string)},
                    {"UROWID", typeof (string)},
                    {"XMLTYPE", typeof (string)},
                    {"INTERVAL YEAR TO MONTH", typeof (string)},
                    {"LONG", typeof (string)},
                    {"FLOAT", typeof (decimal)},
                    {"REAL", typeof (decimal)},
                    {"BINARY_FLOAT", typeof (decimal)},
                    {"BINARY_DOUBLE", typeof (decimal)},
                    {"TIMESTAMP WITH TIME ZONE", typeof (DateTime)},
                    {"TIMESTAMP WITH LOCAL TIME ZONE", typeof (DateTime)},
                    {"INTERVAL DAY TO SECOND", typeof (DateTime)},
                    {"REF CURSOR", typeof (object)},
                    {"PL/SQL BOOLEAN", typeof (bool)},
                    {"UNDEFINED", typeof(string)}
                };

        public static DbType FromDataType(string dataType)
        {
            DbType dbType;
            var success = _dbTypes.TryGetValue(dataType, out dbType);
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