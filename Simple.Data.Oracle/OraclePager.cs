using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Text;
using System.Text.RegularExpressions;
using Simple.Data.Ado;

namespace Simple.Data.Oracle
{
    [Export(typeof(IQueryPager))]
    public class OraclePager : IQueryPager
    {

        /*
         * For Oracle limits are simply where rownum < take +1 
         * So these can be applied during custom query building
         * which is much simpler and does not require sql text parsing
         */
        public IEnumerable<string> ApplyLimit(string sql, int take)
        {
            yield return sql;
        }

        /*
         *  http://stackoverflow.com/questions/5541455/implement-oracle-paging-for-any-query
         *  SELECT * FROM (
         *    SELECT a.*, ROWNUM RNUM FROM (**Select * From SomeTable**) a 
         *    WHERE ROWNUM <= 500) b 
              WHERE b.RNUM >= 1
         * */
        public IEnumerable<string> ApplyPaging(string sql, int skip, int take)
        {
            sql = UpdateWithOrderByIfNecessary(sql);
            var sb = new StringBuilder();
            sb.AppendLine("SELECT * FROM (");
            sb.AppendLine("SELECT \"_sd_\".*, ROWNUM RNUM FROM (");
            sb.Append(sql);
            sb.AppendLine(") \"_sd_\"");
            sb.AppendFormat("WHERE ROWNUM <= {0} + {1}) \"_sd2_\" ", skip, take);
            sb.AppendFormat("WHERE \"_sd2_\".RNUM > {0}", skip);

            yield return sb.ToString();
        }

        private static string UpdateWithOrderByIfNecessary(string sql)
        {
            if (sql.IndexOf("order by ", StringComparison.InvariantCultureIgnoreCase) != -1)
                return sql;
            var col = GetFirstColumn(sql);
            return sql + " ORDER BY " + col;
        }

        private static string GetFirstColumn(string sql)
        {
            var idx1 = sql.IndexOf("select") + 7;
            var idx2 = sql.IndexOf(",", idx1);
            return sql.Substring(idx1, idx2 - 7).Trim();
        }
    }
}