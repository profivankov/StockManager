using System.Data.SQLite;
using System.IO;

namespace StockManager.Tests
{
    public static class DatabaseHelper
    {
        private static readonly string DbPath = @"C:\sqlite\test.db";
        private static readonly string ConnectionString = $"Data Source={DbPath};Version=3;";

        public static void InitializeDatabase()
        {
            if (!File.Exists(DbPath))
            {
                SQLiteConnection.CreateFile(DbPath);
            }

            using (var connection = new SQLiteConnection(ConnectionString))
            {
                connection.Open();

                string createTableQuery = @"
                CREATE TABLE IF NOT EXISTS StockItems (
                    Isin TEXT PRIMARY KEY,
                    Name TEXT NOT NULL,
                    Quantity INTEGER,
                    Price DECIMAL
                );
            ";

                using (var command = new SQLiteCommand(createTableQuery, connection))
                {
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}
