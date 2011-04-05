using System.IO;

namespace Simple.Data.Oracle.ReflectionSql
{
    public static class SqlLoader
    {
        public static string UserTablesAndViews
        {
            get { return LoadFile("user_tables_views.txt"); }
        }

        private static Stream GetStream(string name)
        {
            return typeof(SqlLoader).Assembly
              .GetManifestResourceStream(typeof(SqlLoader), name);
        }

        private static string LoadFile(string name)
        {
            Stream stream = GetStream(name);
            var sr = new StreamReader(stream);
            return sr.ReadToEnd();
        }
    }
}