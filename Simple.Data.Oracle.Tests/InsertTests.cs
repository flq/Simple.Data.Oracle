using System;
using System.Transactions;
using NUnit.Framework;
using Simple.Data.Ado;

namespace Simple.Data.Oracle.Tests
{
    public class Region
    {
        public decimal RegionId { get; set; }
        public string RegionName { get; set; }
        public DateTime CreateDate { get; set; }
    }

    public class Employee
    {
        public decimal EmployeeId { get; set; }
        public decimal? DepartmentId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
    }

    [TestFixture]
    internal class InsertTests : OracleConnectivityContext
    {
        [TestFixtureSetUp]
        public void Given()
        {
            InitDynamicDB();
        }

        [TearDown]
        public void Teardown()
        {
            _db.Regions.DeleteByRegionId(5);
        }

        [Test]
        public void region_was_inserted()
        {
            InsertRegion();
            var r = (Region)_db.Regions.FindByRegionId(5);
            Assert.IsNotNull(r);
            Assert.AreEqual("Antarctica", r.RegionName);
        }

        [Test]
        public void update_works_correctly()
        {
            InsertRegion();
            _db.Regions.UpdateByRegionId(RegionId: 5, RegionName: "Southern Germany");
            var r = (Region)_db.Regions.FindByRegionId(5);
            Assert.IsNotNull(r);
            Assert.AreEqual("Southern Germany", r.RegionName);
        }

        [Test]
        public void update_all_works_correctly()
        {
            InsertRegion();
            _db.Regions.UpdateAll(_db.Regions.RegionId >= 5, RegionName: "Southern Germany");
            var r = (Region)_db.Regions.FindByRegionId(5);
            Assert.IsNotNull(r);
            Assert.AreEqual("Southern Germany", r.RegionName);
        }

        [Test]
        public void transaction_works_correctly()
        {
            using (var tx = _db.BeginTransaction())
            {
                tx.Regions.Insert(RegionId: 5, RegionName: "Awesomnia");
                var r = (Region)tx.Regions.FindByRegionId(5);
                Assert.IsNotNull(r);
                Assert.AreEqual("Awesomnia", r.RegionName);
            }

            var rgn = _db.Regions.FindByRegionId(5);
            Assert.IsNull(rgn);
        }

        [Test]
        public void the_default_datetime_of_db_is_returned()
        {
            var r = (Region)_db.Regions.Insert(RegionId: 5, RegionName: "Antarctica");
            Assert.That(DateTime.Now - r.CreateDate, Is.AtMost(TimeSpan.FromSeconds(2)));
        }

        [Test]
        public void usage_of_sequence_for_insert()
        {
            using (var tx = _db.BeginTransaction())
            {
                var d = tx.Departments.Insert(DepartmentId: Sequence.Next("DEPARTMENTS_SEQ"), DepartmentName: "My new Board", ManagerId: 100, LocationId: 1000);
                Assert.That(d.DepartmentId, Is.AtLeast(100));
            }
        }

        [Test]
        public void usage_of_next_and_current_value()
        {
            using (var tx = _db.BeginTransaction())
            {
                tx.Departments.Insert(
                  DepartmentId: Sequence.Next("DEPARTMENTS_SEQ"),
                  DepartmentName: "Sky",
                  ManagerId: 100,
                  LocationId: 1000);
                tx.Employees.Insert(
                  EmployeeId: Sequence.Next("EMPLOYEES_SEQ"), 
                  LastName: "Brannigan", 
                  Email: "awesome@gmail.com", 
                  HireDate: new DateTime(2011,1,1),
                  JobId: "AD_ASST",
                  DepartmentId: Sequence.Current("DEPARTMENTS_SEQ"));
                var d = tx.Departments.Find(tx.Departments.Employees.LastName == "Brannigan");
                Assert.AreEqual("Sky", d.DepartmentName);
            }
        }

        [Test]
        public void TestSequenceInsertUsingSharedConnectionAndTransactionScope()
        {
            using (var scope = new TransactionScope(TransactionScopeOption.RequiresNew))
            {
                var adapter = _db.GetAdapter() as AdoAdapter;
                adapter.UseSharedConnection(adapter.ConnectionProvider.CreateConnection());

                _db.Departments.Insert(
                  DepartmentId: Sequence.Next("DEPARTMENTS_SEQ"),
                  DepartmentName: "Sky",
                  ManagerId: 100,
                  LocationId: 1000);
                _db.Employees.Insert(
                  EmployeeId: Sequence.Next("EMPLOYEES_SEQ"),
                  LastName: "Brannigan",
                  Email: "awesome@gmail.com",
                  HireDate: new DateTime(2011, 1, 1),
                  JobId: "AD_ASST",
                  DepartmentId: Sequence.Current("DEPARTMENTS_SEQ"));
                var d = _db.Departments.Find(_db.Departments.Employees.LastName == "Brannigan");
                Assert.AreEqual("Sky", d.DepartmentName);
                adapter.StopUsingSharedConnection();
            }
        }

        [Test]
        public void return_of_insertion_maps_dbnull_to_null()
        {
            using (var tx = _db.BeginTransaction())
            {
                var e = (Employee)tx.Employees.Insert(
                  EmployeeId: Sequence.Next("EMPLOYEES_SEQ"),
                  LastName: "Brannigan",
                  Email: "awesome@gmail.com",
                  HireDate: new DateTime(2011, 1, 1),
                  JobId: "AD_ASST");
                
                Assert.That(e.EmployeeId, Is.AtLeast(100));
                Assert.AreEqual("Brannigan", e.LastName);
                Assert.IsNull(e.FirstName);
                Assert.IsFalse(e.DepartmentId.HasValue);
            }
        }

        private void InsertRegion()
        {
            _db.Regions.Insert(new Region {RegionId = 5m, RegionName = "Antarctica"});
        }
    }
}