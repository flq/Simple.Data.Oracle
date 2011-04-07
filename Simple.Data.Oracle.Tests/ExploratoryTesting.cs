using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Simple.Data.Oracle.Tests
{
    [TestFixture]
    internal class ExploratoryTesting : OracleConnectivityContext
    {
        [TestFixtureSetUp]
        public void Given()
        {
            InitDynamicDB();
        }

        [Test]
        public void employee_by_name()
        {
            IEnumerable<dynamic> emps = _db.Employees.FindAllByFirst_NameAndLast_Name("Steven", "King");
            Assert.AreEqual(1, emps.Count());
            var emp = emps.First();
            Assert.AreEqual("SKING", emp.Email);
        }

        [Test]
        public void employees_via_job_min_salary()
        {
            try
            {
                IEnumerable<dynamic> employees = _db.Employees.Find(_db.Employees.Jobs.Min_Salary == 20000);
                Assert.AreEqual(1, employees.Count());
            }
            catch (MissingMethodException x)
            {
                Assert.Fail(x.Message);
            }
        }
    }
}