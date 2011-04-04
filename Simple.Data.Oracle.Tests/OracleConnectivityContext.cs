using NUnit.Framework;

namespace Simple.Data.Oracle.Tests
{
    public class OracleConnectivityContext
    {
        protected dynamic _db;
        protected const string ConnectionString = "Data Source=XE;User id=hr;Password=hr";

        [TestFixtureSetUp]
        public void Given()
        {
            _db = Database.OpenConnection(ConnectionString);
        }
    }
}