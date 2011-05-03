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
            IEnumerable<dynamic> emps = _db.Employees.FindAllByFirstNameAndLastName("Steven", "King").OfType<dynamic>();
            Assert.AreEqual(1, emps.Count());
            var emp = emps.First();
            Assert.AreEqual("SKING", emp.Email);
        }

        [Test]
        public void employees_via_job_min_salary()
        {
            try
            {
                IEnumerable<dynamic> employees = _db.Employees.FindAll(_db.Employees.Jobs.MinSalary == 20000).OfType<dynamic>();
                Assert.AreEqual(1, employees.Count());
            }
            catch (MissingMethodException x)
            {
                Assert.Fail(x.Message);
            }
        }
    }
}