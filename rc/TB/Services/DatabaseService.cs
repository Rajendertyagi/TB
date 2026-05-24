using Dapper;
using Microsoft.Data.Sqlite;
using System;
using System.IO;
using TB.Models;

namespace TB.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService()
        {
            // Fulfilling the requirement: Local /Downloads directory at executable root
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

            // Tabs Table (For restoring active sessions)
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

            // History Table (For 30-day rolling history)
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS History (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Url TEXT,
                    Title TEXT,
                    VisitDate DATETIME
                )");

            // Settings Table (Key-Value store for preferences)
            connection.Execute(@"
                CREATE TABLE IF NOT EXISTS Settings (
                    Key TEXT PRIMARY KEY,
                    Value TEXT
                )");
        }

        // Upsert method: Inserts a new tab or updates an existing one
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
    }
}
