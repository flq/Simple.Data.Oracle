using NUnit.Framework;
using System.Linq;
using Simple.Data.Ado.Schema;

namespace Simple.Data.Oracle.Tests
{
    [TestFixture]
    internal class ObtainingTableMetadata : OracleConnectivityContext
    {
        private OracleSchemaProvider _sql;

        [TestFixtureSetUp]
        public void Given()
        {
            _sql = GetSchemaProvider();
        }

        [Test]
        public void correct_number_of_items()
        {
            Assert.That(Tables, Has.Count.EqualTo(9));
        }

        [Test]
        public void found_the_view()
        {
            var vw = Tables.Single(t => t.Type == TableType.View);
            Assert.AreEqual("EMP_DETAILS_VIEW", vw.ActualName);
        }

        [Test]
        public void found_columns_to_table()
        {
            var table = TableByName("COUNTRIES");
            var cols = _sql.GetColumns(table).ToList();
            Assert.That(cols, Has.Count.EqualTo(3));
            Assert.AreEqual("COUNTRY_ID", cols[0].ActualName);
        }

        [Test]
        public void found_columns_to_view()
        {
            var table = TableByName("EMP_DETAILS_VIEW");
            var cols = _sql.GetColumns(table).ToList();
            Assert.That(cols, Has.Count.EqualTo(16));
        }

        [Test]
        public void identifies_primary_key()
        {
            var table = TableByName("COUNTRIES");
            var keys = _sql.GetPrimaryKey(table);
            Assert.AreEqual("COUNTRY_ID", keys[0]);
        }

        [Test]
        public void composite_key_supported()
        {
            var table = TableByName("JOB_HISTORY");
            var keys = _sql.GetPrimaryKey(table);
            Assert.AreEqual("EMPLOYEE_ID", keys[0]);
            Assert.AreEqual("START_DATE", keys[1]);
        }

        [Test]
        public void all_foreign_keys_of_job_history()
        {
            var table = TableByName("JOB_HISTORY");
            var fks = _sql.GetForeignKeys(table).ToList();
            Assert.That(fks, Has.Count.EqualTo(3));
        }


    }
}