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
        private static readonly Dictionary<string, DbType> _dbToDbtype =
            new Dictionary<string, DbType>(StringComparer.InvariantCultureIgnoreCase)
                {
                    {"CHAR", DbType.StringFixedLength},
                    {"NCHAR", DbType.StringFixedLength},
                    {"VARCHAR2", DbType.String},
                    {"NVARCHAR2", DbType.String},
                    {"CLOB", DbType.String},
                    {"NCLOB", DbType.String},
                    {"ROWID", DbType.String},
                    {"UROWID", DbType.String},
                    {"XMLTYPE", DbType.String},
                    {"INTERVAL YEAR TO MONTH", DbType.String},
                    {"LONG", DbType.String},
                    {"FLOAT", DbType.Decimal},
                    {"REAL", DbType.Decimal},
                    {"BINARY_FLOAT", DbType.Decimal},
                    {"BINARY_DOUBLE", DbType.Decimal},
                    {"DATE", DbType.Date},
                    {"TIMESTAMP", DbType.Date},
                    {"TIMESTAMP WITH TIME ZONE", DbType.Date},
                    {"TIMESTAMP WITH LOCAL TIME ZONE", DbType.Date},
                    {"INTERVAL DAY TO SECOND", DbType.Time},
                    {"BLOB", DbType.Binary},
                    {"PL/SQL BOOLEAN", DbType.Boolean},
                    {"REF CURSOR", DbType.Object},
                };

        private static readonly Dictionary<string, Type> _dbToClr =
            new Dictionary<string, Type>
                {
                    {"CHAR", typeof (string)},
                    {"NCHAR", typeof (string)},
                    {"VARCHAR2", typeof (string)},
                    {"NVARCHAR2", typeof (string)},
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
                    {"DATE", typeof (DateTime)},
                    {"TIMESTAMP", typeof (DateTime)},
                    {"TIMESTAMP WITH TIME ZONE", typeof (DateTime)},
                    {"TIMESTAMP WITH LOCAL TIME ZONE", typeof (DateTime)},
                    {"INTERVAL DAY TO SECOND", typeof (DateTime)},
                    {"BLOB", typeof (byte[])},
                    {"PL/SQL BOOLEAN", typeof (bool)},
                    {"REF CURSOR", typeof (object)},
                };

        private static readonly Tuple<Func<int, int, bool>, DbType, Type>[] _numberTypes =
                {
                    new Tuple<Func<int, int, bool>, DbType, Type>((precision,scale) => scale == 0 && precision == 1, DbType.Boolean, typeof(bool)),
                    new Tuple<Func<int, int, bool>, DbType, Type>((precision,scale) => scale == 0 && precision >= 2 && precision <= 9, DbType.Int32, typeof(int)),
                    new Tuple<Func<int, int, bool>, DbType, Type>((precision,scale) => scale == 0 && precision >= 10 && precision <= 18, DbType.Int64, typeof(long)),
                    new Tuple<Func<int, int, bool>, DbType, Type>((precision,scale) => scale >= 1 && scale <= 15, DbType.Double, typeof(double)),
                    new Tuple<Func<int, int, bool>, DbType, Type>((precision,scale) => true, DbType.Decimal, typeof(decimal)),
                };

        private static readonly Tuple<Func<int, int, bool>, DbType, Type>[] _rawTypes =
                {
                    new Tuple<Func<int, int, bool>, DbType, Type>((precision,scale) => precision == 16, DbType.Guid, typeof(Guid)),
                    new Tuple<Func<int, int, bool>, DbType, Type>((precision,scale) => true, DbType.Binary, typeof(byte[])),
                };

        private static readonly Dictionary<string, IEnumerable<Tuple<Func<int, int, bool>, DbType, Type>>> _typesWithCardinality =
            new Dictionary<string, IEnumerable<Tuple<Func<int, int, bool>, DbType, Type>>>(StringComparer.InvariantCultureIgnoreCase)
                {
                    {"NUMBER", _numberTypes},
                    {"RAW", _rawTypes},
                };

        private static bool GetTypeWithCardinality(string dataType, int dataPrecision, int dataScale, out DbType dbType, out Type type)
        {
            dbType = DbType.Object;
            type = typeof(object);
            IEnumerable<Tuple<Func<int, int, bool>, DbType, Type>> typesWithCardinality;
            if (_typesWithCardinality.TryGetValue(dataType, out typesWithCardinality))
            {
                foreach (var typeWithCardinality in typesWithCardinality)
                {
                    if (typeWithCardinality.Item1(dataPrecision, dataScale))
                    {
                        dbType = typeWithCardinality.Item2;
                        type = typeWithCardinality.Item3;
                        return true;
                    }
                }
            }
            return false;
        }

        public static DbType FromDataType(string dataType, int dataPrecision, int dataScale)
        {
            DbType dbType;
            Type type;
            var success = _dbToDbtype.TryGetValue(dataType, out dbType);
            if (!success)
            {
                success = GetTypeWithCardinality(dataType, dataPrecision, dataScale, out dbType, out type);
            }

            return success ? dbType : DbType.Object;
        }

        public static Type ToClrType(this string dataType, int dataPrecision, int dataScale)
        {
            DbType dbType;
            Type type;
            var success = _dbToClr.TryGetValue(dataType.ToUpperInvariant(), out type);
            if (!success)
            {
                success = GetTypeWithCardinality(dataType, dataPrecision, dataScale, out dbType, out type);
            }

            if (!success)
                throw new ArgumentException("Oracle type " + dataType + " could not be mapped to clr type.");
            return type;
        }
    }
}
