using System.Configuration;
using System.Linq;
using NUnit.Framework;

namespace Simple.Data.Oracle.Tests
{
    [TestFixture]
    internal class CustomSchemaConfigurationConnectivity 
    {

#if DEVART
        const string ConnectionName = "DevartOracleOther";
#endif
#if !DEVART
        const string ConnectionName = "OracleClientOther";
#endif

        public class CustomSchemaConfiguration : ISchemaConfiguration
        {
            public string Schema { get { return "hr";  } }
        }

        [Test]
        public void Basic_retrival_of_tables()
        {
            var connectionString = ConfigurationManager.ConnectionStrings[ConnectionName].ConnectionString;

            var provider = new OracleConnectionProvider();
            provider.SetConnectionString(connectionString);

            var connectionProvider = provider;
            var schemaProvider = new OracleSchemaProvider(connectionProvider, new CustomSchemaConfiguration());
            var tables = schemaProvider.GetTables().ToList();

            Assert.Contains("REGIONS", tables.Select(t=>t.ActualName).ToList());
            Assert.Contains("DEPARTMENTS", tables.Select(t => t.ActualName).ToList());
            Assert.Contains("EMPLOYEES", tables.Select(t => t.ActualName).ToList());
            Assert.Contains("JOB_HISTORY", tables.Select(t => t.ActualName).ToList());
            Assert.Contains("JOBS", tables.Select(t => t.ActualName).ToList());
            Assert.Contains("LOCATIONS", tables.Select(t => t.ActualName).ToList());
        }        
    }
}
