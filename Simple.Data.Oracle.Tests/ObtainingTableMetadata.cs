using System;
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
        private SqlReflection _sql;

        [TestFixtureSetUp]
        public void Given()
        {
            _sql = GetSqlReflection();
            _tables = _sql.UserTables().ToList();
        }

        [Test]
        public void correct_number_of_items()
        {
            Assert.That(_tables, Has.Count.EqualTo(9));
        }

        [Test]
        public void found_the_view()
        {
            var vw = _tables.Single(t => t.Type == TableType.View);
            Assert.AreEqual("EMP_DETAILS_VIEW", vw.ActualName);
        }

        [Test]
        public void found_columns_to_table()
        {
            var table = _tables.Single(t => t.ActualName.InvariantEquals("COUNTRIES"));
            var cols = _sql.Columns(table).ToList();
            Assert.That(cols, Has.Count.EqualTo(3));
            Assert.AreEqual("COUNTRY_ID", cols[0].ActualName);
        }

        [Test]
        public void identifies_primary_key()
        {
            var prov = new OracleSchemaProvider(ConstructProvider());
            var table = _tables.Single(t => t.ActualName.InvariantEquals("COUNTRIES"));
            var keys = prov.GetPrimaryKey(table);
            Assert.AreEqual("COUNTRY_ID", keys[0]);
        }

        [Test]
        public void composite_key_supported()
        {
            var prov = new OracleSchemaProvider(ConstructProvider());
            var table = _tables.Single(t => t.ActualName.InvariantEquals("JOB_HISTORY"));
            var keys = prov.GetPrimaryKey(table);
            Assert.AreEqual("EMPLOYEE_ID", keys[0]);
            Assert.AreEqual("START_DATE", keys[1]);
        }


    }
}