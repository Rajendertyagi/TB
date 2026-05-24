using Microsoft.Data.Sqlite;
using Dapper;
using System.IO;

namespace TB.Data;

public static class DatabaseHelper
{
    private static readonly string DataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".tb_data");
    private static readonly string DbPath = Path.Combine(DataDir, "TB.db");
    public static string ConnectionString => $"Data Source={DbPath}";

    public static void Initialize()
    {
        // Ensure the isolated storage directory exists
        if (!Directory.Exists(DataDir)) Directory.CreateDirectory(DataDir);

        using var connection = new SqliteConnection(ConnectionString);
        
        // Initialize Schema
        connection.Execute(@"
            CREATE TABLE IF NOT EXISTS Sessions (...); -- Schema as defined above
            CREATE TABLE IF NOT EXISTS History (...);
            CREATE TABLE IF NOT EXISTS SystemLogs (...);
            CREATE TABLE IF NOT EXISTS Settings (...);
        ");

        // Maintain 30-day rolling history purge
        connection.Execute("DELETE FROM History WHERE Timestamp < date('now', '-30 days')");
    }
}
