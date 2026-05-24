using Microsoft.Data.Sqlite;
using Dapper;
using System.IO;
using System;

namespace TB.Data;

public static class DatabaseHelper
{
    // FIX: Removed the leading period from the folder name
    private static readonly string DbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tb_data", "TB.db");
    public static string ConnectionString => $"Data Source={DbPath}";

    public static void Initialize()
    {
        Directory.CreateDirectory(Path.GetDirectoryName(DbPath));
        using var connection = new SqliteConnection(ConnectionString);
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Sessions (TabId TEXT PRIMARY KEY, Url TEXT, Title TEXT);
            CREATE TABLE IF NOT EXISTS Settings (Key TEXT PRIMARY KEY, Value TEXT);
            INSERT OR IGNORE INTO Settings (Key, Value) VALUES ('SearchEngine', 'Google');");
    }
}
