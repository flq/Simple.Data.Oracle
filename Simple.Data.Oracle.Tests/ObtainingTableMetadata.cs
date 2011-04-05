using System.Collections.Generic;
using NUnit.Framework;
using System.Linq;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Oracle.Tests
{
    [TestFixture]
    internal class ObtainingTableMetadata : OracleConnectivityContext
    {
        private List<Table> _tables;

        [TestFixtureSetUp]
        public void Given()
        {
            var sql = new SqlReflection(ConstructProvider());
            _tables = sql.UserTables().ToList();
        }

        [Test]
        public void correct_number_of_items()
        {
            Assert.That(_tables, Has.Count.EqualTo(8));
        }

        [Test]
        public void found_the_view()
        {
            var vw = _tables.Single(t => t.Type == TableType.View);
            Assert.AreEqual("EMP_DETAILS_VIEW", vw.ActualName);
        }


    }
}