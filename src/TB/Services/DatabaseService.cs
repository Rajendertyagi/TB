using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using TB.Models;

namespace TB.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string downloadsDir = Path.Combine(baseDir, "Downloads");
            
            if (!Directory.Exists(downloadsDir))
            {
                Directory.CreateDirectory(downloadsDir);
            }

            string dbPath = Path.Combine(downloadsDir, "settings.db");
            _connectionString = $"Data Source={dbPath}";
            
            InitializeDatabase();
        }

        private void InitializeDatabase()
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Open();

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS Tabs (
                    TabId TEXT PRIMARY KEY,
                    Url TEXT,
                    Title TEXT,
                    FaviconUrl TEXT,
                    IsActive INTEGER,
                    IsPinned INTEGER,
                    IsSuspended INTEGER,
                    ScrollX REAL,
                    ScrollY REAL,
                    ZoomLevel REAL,
                    NavigationStack TEXT
                )");

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS History (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Url TEXT,
                    Title TEXT,
                    VisitDate DATETIME
                )");

            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS Settings (
                    Key TEXT PRIMARY KEY,
                    Value TEXT
                )");
        }

        public void SaveTabState(TabState tab)
        {
            using var connection = new SqliteConnection(_connectionString);
            var sql = @"
                INSERT INTO Tabs (TabId, Url, Title, FaviconUrl, IsActive, IsPinned, IsSuspended, ScrollX, ScrollY, ZoomLevel, NavigationStack)
                VALUES (@TabId, @Url, @Title, @FaviconUrl, @IsActive, @IsPinned, @IsSuspended, @ScrollX, @ScrollY, @ZoomLevel, @NavigationStack)
                ON CONFLICT(TabId) DO UPDATE SET
                    Url = excluded.Url,
                    Title = excluded.Title,
                    FaviconUrl = excluded.FaviconUrl,
                    IsActive = excluded.IsActive,
                    IsPinned = excluded.IsPinned,
                    IsSuspended = excluded.IsSuspended,
                    ScrollX = excluded.ScrollX,
                    ScrollY = excluded.ScrollY,
                    ZoomLevel = excluded.ZoomLevel,
                    NavigationStack = excluded.NavigationStack;";
            
            connection.Execute(sql, tab);
        }

        public IEnumerable<TabState> GetSavedTabs()
        {
            using var connection = new SqliteConnection(_connectionString);
            return connection.Query<TabState>("SELECT * FROM Tabs");
        }

        public void DeleteTabState(Guid tabId)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Execute("DELETE FROM Tabs WHERE TabId = @TabId", new { TabId = tabId });
        }

        public void AddToHistory(string url, string title)
        {
            using var connection = new SqliteConnection(_connectionString);
            connection.Execute(@"
                INSERT INTO History (Url, Title, VisitDate) 
                VALUES (@Url, @Title, @VisitDate)", 
                new { Url = url, Title = title, VisitDate = DateTime.UtcNow });
        }

        public void PurgeOldHistory()
        {
            using var connection = new SqliteConnection(_connectionString);
            var cutoffDate = DateTime.UtcNow.AddDays(-30);
            connection.Execute("DELETE FROM History WHERE VisitDate < @Cutoff", new { Cutoff = cutoffDate });
        }
    }
}
