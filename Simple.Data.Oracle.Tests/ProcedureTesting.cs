using NUnit.Framework;

namespace Simple.Data.Oracle.Tests
{
    [TestFixture]
    internal class ProcedureTesting: OracleConnectivityContext
    {

        [TestFixtureSetUp]
        public void Given()
        {
            InitDynamicDB();
        }

        [Test]
        public void employee_count_in_department_is_callable()
        {
            var count = (decimal) _db.Employee_Count_Department("Marketing").First();
            Assert.AreEqual(2, count);
        }
    }
}