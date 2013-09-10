using System.Configuration;
using System.Linq;

namespace Simple.Data.Oracle
{   
    internal class DefaultSchemaConfiguration : ISchemaConfiguration
    {
        private readonly OracleConnectionProvider _provider;

        public DefaultSchemaConfiguration(OracleConnectionProvider provider)
        {
            _provider = provider;
        }

        public string Schema
        {
            get
            {
                return ConfigurationManager.AppSettings.AllKeys.Contains("Simple.Data.Oracle.Schema")
                           ? ConfigurationManager.AppSettings["Simple.Data.Oracle.Schema"]
                           : _provider.UserOfConnection;
            }
        }
    }
}