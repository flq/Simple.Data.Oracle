using System;
using System.Data;
using NUnit.Framework;
#if DEVART
using Devart.Data.Oracle;
#endif

namespace Simple.Data.Oracle.Tests
{
    [TestFixture]
    internal class RawOracleTest : OracleConnectivityContext
    {

        [TearDown]
        public void TearDown()
        {
            using (var c = GetCommand("delete from regions where region_id = 6"))
            {
                c.Connection.Open();
                c.ExecuteNonQuery();
            }
        }

        [Test]
        public void check_of_returning_statement()
        {
            var sql = @"insert into regions (region_id, region_name) values (6, 'Antarctica') returning ""REGION_ID"", ""REGION_NAME"", ""CREATE_DATE"" into :p1, :p2, :p3";
            using (var c = GetCommand(sql))
            {
                var p1 = c.CreateParameter();
                p1.ParameterName = "p1";
                p1.Value = 1m;
                p1.Direction = ParameterDirection.Output;
                var p2 = c.CreateParameter();
                p2.ParameterName = "p2";
                p2.Value = "a";
                p2.Size = 25;
                p2.Direction = ParameterDirection.Output;
                var p3 = c.CreateParameter();
                p3.ParameterName = "p3";
                p3.Value = DateTime.MinValue;
                p3.Direction = ParameterDirection.Output;
                c.Parameters.Add(p1);
                c.Parameters.Add(p2);
                c.Parameters.Add(p3);
                c.Connection.Open();
                c.ExecuteNonQuery();

                Assert.AreEqual(6, c.Parameters["p1"].Value);
                Assert.AreEqual("Antarctica", c.Parameters["p2"].Value);
                var timeSpan = (DateTime.Now - ((DateTime)c.Parameters["p3"].Value));
                Console.WriteLine(timeSpan.TotalMilliseconds);
                Assert.That(timeSpan.TotalMilliseconds, Is.AtMost(2000));
            }
        }        
    }
}