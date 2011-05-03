using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Simple.Data.Oracle.Tests
{
    [TestFixture]
    internal class PagingTest : OracleConnectivityContext
    {
        private dynamic _basicQueryWithOrder;
        private dynamic _basicQuery;

        [TestFixtureSetUp]
        public void Given()
        {
            InitDynamicDB();
            _basicQuery = _db.Employees.All();
            _basicQueryWithOrder = _db.Employees.All().OrderBy(_db.Employees.Last_Name);
        }

        [Test]
        public void paging_call_without_skip()
        {
            IEnumerable<dynamic> q = _basicQueryWithOrder.Take(10).ToList<dynamic>();
            Assert.IsNotNull(q);
            Assert.AreEqual(10, q.Count());
            Assert.AreEqual("Abel", q.First().LastName);
            Assert.AreEqual("Bernstein", q.Last().LastName);
        }

        [Test]
        public void paging_call_with_skip()
        {
            IEnumerable<dynamic> q = _basicQueryWithOrder.Skip(10).Take(10).ToList<dynamic>();
            Assert.IsNotNull(q);
            Assert.AreEqual(10, q.Count());
            Assert.AreEqual("Bissot", q.First().LastName);
            Assert.AreEqual("Davies", q.Last().LastName);
        }

        [Test]
        public void paging_without_order_oders_on_first_column()
        {
            // This test wouldn't fail even if there is no order by, so sadly we can only test
            // that the code runs without errors.
            IEnumerable<dynamic> q = _basicQuery.Skip(10).Take(10).ToList<dynamic>();
            Assert.IsNotNull(q);
            Assert.AreEqual(10, q.Count());
        }
    }
}