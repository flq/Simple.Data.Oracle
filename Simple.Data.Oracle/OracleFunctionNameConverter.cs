using System;
using Simple.Data.Ado;

namespace Simple.Data.Oracle
{
    class OracleFunctionNameConverter : IFunctionNameConverter
    {
        public string ConvertToSqlName(string simpleFunctionName)
        {
            if (simpleFunctionName.Equals("average", StringComparison.InvariantCultureIgnoreCase))
            {
                return "avg";
            }
            return simpleFunctionName;
        }
    }
}
