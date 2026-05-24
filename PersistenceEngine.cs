using System;
using System.IO;
using Microsoft.Data.Sqlite;

namespace TradingBrowser
{
    public static class PersistenceEngine
    {
        private static readonly string DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userdata.db");

        public static void Initialize()
        {
            try
            {
                using var conn = new SqliteConnection($"Data Source={DbPath}");
                conn.Open();
                
                string createTables = "CREATE TABLE IF NOT EXISTS store (key TEXT PRIMARY KEY, val TEXT);";
                using var cmd = new SqliteCommand(createTables, conn);
                cmd.ExecuteNonQuery();
                
                TradingBrowser.Logger.WriteLog("Persistence Engine", "SQLite database infrastructure initialized successfully.");
            }
            catch (Exception ex)
            {
                TradingBrowser.Logger.WriteLog("Global Errors", $"SQLite Init Failure: {ex.Message}");
            }
        }
    }
}
