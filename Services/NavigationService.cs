using System;
using TB.Data;
using Dapper;
using Microsoft.Data.Sqlite;

namespace TB.Services;

public class NavigationService
{
    // Fetches your preferred engine from the SQLite Settings table
    public string GetSearchUrl(string query)
    {
        using var connection = new SqliteConnection(DatabaseHelper.ConnectionString);
        var engine = connection.QueryFirstOrDefault<string>("SELECT Value FROM Settings WHERE Key = 'SearchEngine'") ?? "Google";
        
        string baseUrl = engine == "Bing" ? "https://www.bing.com/search?q=" : "https://google.com/search?q=";
        return Uri.IsWellFormedUriString(query, UriKind.Absolute) ? query : $"{baseUrl}{Uri.EscapeDataString(query)}";
    }

    // Handles JS-to-C# Error Forwarding
    public void LogJsError(string tabId, string errorDetails)
    {
        using var connection = new SqliteConnection(DatabaseHelper.ConnectionString);
        connection.Execute("INSERT INTO SystemLogs (Category, Message, TabId) VALUES ('JS_Error', @Message, @TabId)", 
            new { Message = errorDetails, TabId = tabId });
    }
}
