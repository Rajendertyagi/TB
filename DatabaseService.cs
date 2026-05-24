using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Data.Sqlite;
using Dapper;

namespace TradingBrowser
{
    // A lightweight data structure representing a saved browser tab layout
    public class SavedTabItem
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Url { get; set; }
        public long LastAccessedTicks { get; set; }
    }

    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            // Position the database inside the local app data folder to prevent write privilege issues
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string dbDirectory = Path.Combine(appDataFolder, "TradingBrowser");
            Directory.CreateDirectory(dbDirectory);
            
            string dbPath = Path.Combine(dbDirectory, "browser_cache.db");
            _connectionString = $"Data Source={dbPath};";

            InitializeDatabaseStructure();
        }

        private void InitializeDatabaseStructure()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                // Execute a blazing-fast raw query to set up the workspace state table
                string sql = @"
                    CREATE TABLE IF NOT EXISTS SavedTabs (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Url TEXT NOT NULL,
                        LastAccessedTicks INTEGER NOT NULL
                    );";
                
                connection.Execute(sql);
            }
        }

        /// <summary>
        /// Saves a collection of running tabs using a lightning-fast transactional batch statement.
        /// </summary>
        public void SaveWorkspaceLayout(IEnumerable<SavedTabItem> tabs)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    // Wipe the old session state cleanly
                    connection.Execute("DELETE FROM SavedTabs", transaction: transaction);

                    // Dapper Magic: Automatically loops through the collection, maps properties to parameters, 
                    // and executes the compiled command at native speed with zero reflection overhead.
                    string insertSql = "INSERT INTO SavedTabs (Title, Url, LastAccessedTicks) VALUES (@Title, @Url, @LastAccessedTicks);";
                    connection.Execute(insertSql, tabs, transaction: transaction);

                    transaction.Commit();
                }
            }
        }

        /// <summary>
        /// Loads the previous layout on application startup in a single line.
        /// </summary>
        public List<SavedTabItem> LoadWorkspaceLayout()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                string sql = "SELECT * FROM SavedTabs ORDER BY LastAccessedTicks ASC;";
                
                // Dapper Magic: Queries the database and automatically converts rows into strongly-typed C# objects instantly.
                return connection.Query<SavedTabItem>(sql).ToList();
            }
        }
    }
}
