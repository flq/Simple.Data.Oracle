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
            IEnumerable<dynamic> emps = _db.Employees.FindAllByFirstNameAndLastName("Steven", "King").ToList();
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

        [Test]
        public void ordered_employees_by_date_range()
        {
            List<dynamic> employees = _db.Employees.FindAllByHireDate("1995-01-01".to("1996-01-01")).OrderByHireDateDescending().ToList();
            Assert.AreEqual(4, employees.Count);
            Assert.AreEqual(new DateTime(1995, 10, 17), employees[0].HireDate);
        }

        [Test]
        public void find_employees_with_id_array()
        {
            List<dynamic> employees = _db.Employees.FindAllByEmployeeId(new [] { 100, 101, 102 }).ToList();
            Assert.AreEqual(3, employees.Count);
            Assert.AreEqual("King", employees[0].LastName);
        }

        [Test]
        public void use_of_alias()
        {
            var actual = _db.Employees.QueryByEmployeeId(100).Select(_db.Employees.FirstName.As("Name")).First();
            Assert.AreEqual("Steven", actual.Name);
        }

        [Test]
        public void check_with_exists()
        {
            var result = _db.Employees.ExistsByFirstName("Steven");
            Assert.IsTrue(result);
        }

        [Test]
        public void check_with_exists_2()
        {
            var result = _db.Employees.ExistsByFirstName("Zabloing");
            Assert.IsFalse(result);
        }

        [Test]
        public void self_join()
        {
            var q = _db.Employees.Query().Join(_db.Employees.As("Manager"), EmployeeId: _db.Employees.ManagerId);
            q = q.Select(_db.Employees.LastName, q.Manager.LastName.As("Manager"));
            List<dynamic> employees = q.ToList();

            Assert.AreEqual(106, employees.Count); // The top man is missing

            var kingsSubordinates = employees.Where(e => e.Manager == "King").ToList();

            Assert.AreEqual(14, kingsSubordinates.Count);
        }

        [Test]
        public void filtered_self_join()
        {
            var q = _db.Employees.Query()
                .Join(_db.Employees.As("Manager"), EmployeeId: _db.Employees.ManagerId)
                .Where(_db.Employees.Salary > 15000);
            List<dynamic> employees = q.Select(_db.Employees.LastName, q.Manager.LastName.As("Manager")).ToList();
            Assert.AreEqual(2, employees.Count); // The top man is missing
            Assert.IsTrue(employees.All(e => e.Manager.Equals("King")));
        }

        [Test]
        public void just_returning_a_count()
        {
            int count = _db.Employees.FindAllByEmployeeId(100.to(102)).Count();
            Assert.AreEqual(3, count);
        }


        [Test,Ignore("Known issue with data type")]
        public void page_with_total_count()
        {
            Future<int> count;
            var list = _db.Employees.QueryByEmployeeId(100.to(125))
                .Take(10)
				.WithTotalCount(out count)
                .ToList();

            Assert.IsTrue(count.HasValue);
            Assert.AreEqual(10, list.Count);
            Assert.AreEqual(25, count);
        }

        [Test]
        public void use_of_forUpdate()
        {
            var actual = _db.Employees.QueryByEmployeeId(100).Select(_db.Employees.FirstName).ForUpdate(false).First();
            Assert.AreEqual("Steven", actual.FirstName);
            actual = _db.Employees.QueryByEmployeeId(100).Select(_db.Employees.FirstName).ForUpdate(true).First();
            Assert.AreEqual("Steven", actual.FirstName);
        }
    }
}