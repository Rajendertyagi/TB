using Microsoft.Data.Sqlite;
using Dapper;
using Serilog;

namespace TB
{
    public class DatabaseService
    {
        private const string DbPath = "Data Source=app.db";

        public void Initialize()
        {
            try
            {
                using var connection = new SqliteConnection(DbPath);
                connection.Open();
                
                // Create a sample table
                connection.Execute(@"
                    CREATE TABLE IF NOT EXISTS Logs (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Message TEXT,
                        Timestamp DATETIME
                    )");
                
                Log.Information("Database initialized successfully.");
            }
            catch (System.Exception ex)
            {
                Log.Error(ex, "Failed to initialize database.");
            }
        }
    }
}
