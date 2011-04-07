using NUnit.Framework;

namespace Simple.Data.Oracle.Tests
{
    [TestFixture]
    internal class BasicConnectivity : OracleConnectivityContext
    {
        [Test]
        public void Basic_connection_to_dual()
        {
            Assert.IsNotNull(_db);
            var dual = _db.Dual.All();
            Assert.IsNotNull(dual);
            foreach (Dual d in dual)
            {
                Assert.IsNotNull(d);
                Assert.AreEqual("X", d.Dummy);
            }
        }

        [TestFixtureSetUp]
        public void Given()
        {
            InitDynamicDB();
        }
    }
}
