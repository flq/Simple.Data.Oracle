using System.IO;

namespace Simple.Data.Oracle.ReflectionSql
{
    public static class SqlLoader
    {
        public static string UserTablesAndViews
        {
            get { return LoadFile("user_tables_views.txt"); }
        }

        public static string UserColumns
        {
            get { return LoadFile("user_columns.txt"); }
        }

        public static string PrimaryKeys
        {
            get { return LoadFile("table_pks.txt"); }
        }

        public static string ForeignKeys
        {
            get { return LoadFile("table_fks.txt"); }
        }

        private static string LoadFile(string name)
        {
            Stream stream = GetStream(name);
            var sr = new StreamReader(stream);
            return sr.ReadToEnd();
        }

        private static Stream GetStream(string name)
        {
            return typeof(SqlLoader).Assembly
                .GetManifestResourceStream(typeof(SqlLoader), name);
        }
    }
}