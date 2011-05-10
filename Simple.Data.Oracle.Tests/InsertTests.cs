using System;
using NUnit.Framework;

namespace Simple.Data.Oracle.Tests
{
    public class Region
    {
        public decimal RegionId { get; set; }
        public string RegionName { get; set; }
        public DateTime CreateDate { get; set; }
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

        private void InsertRegion()
        {
            _db.Regions.Insert(new Region {RegionId = 5m, RegionName = "Antarctica"});
        }
    }
}