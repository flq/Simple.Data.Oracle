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
            var result = _db.Employee_Count_Department("Marketing");
            var @return = result.ReturnValue;
            Assert.IsAssignableFrom<decimal>(@return);
            Assert.AreEqual(2, @return);
        }

        [Test]
        public void accessing_package_function_with_double_underscore()
        {
            var result = _db.Department__Department_Count();
            Assert.AreEqual(27, result.ReturnValue);
        }

        [Test]
        public void accessing_second_package_function()
        {
            var result = _db.Department__Manager_Of_Department("Marketing");
            var @return = result.ReturnValue;
            Assert.IsAssignableFrom<string>(@return);
            Assert.AreEqual("Hartstein", @return);
        }

        [Test]
        public void accessing_stored_proc_with_output_values()
        {
            var result = _db.Department__Manager_And_Count("Marketing");
            Assert.AreEqual("Jones from Marketing", result.OutputValues["P_MANAGER"]);
            Assert.AreEqual(10, result.OutputValues["P_COUNT"]);
        }
    }
}