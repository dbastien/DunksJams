using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine;

public static class DataUtils
{
    public static string FallbackLanguage { get; set; } = "en";
    static string _googleApiKey;
    static readonly HttpClient _client = new();

    public static void SetGoogleApiKey(string apiKey) => _googleApiKey = apiKey;

    public static Dictionary<string, string> ParseCsv(string data, string langCode)
    {
        var lineEndIndex = data.IndexOf('\n');
        if (lineEndIndex == -1) throw new ArgumentException("Invalid CSV format.");

        var headers = data[..lineEndIndex].Trim().Split(',');
        var langIndex = Array.IndexOf(headers, langCode);
        langIndex = langIndex != -1 ? langIndex : Array.IndexOf(headers, FallbackLanguage);
        if (langIndex == -1) throw new ArgumentException("Language code not found.");

        return ParseData(data[(lineEndIndex + 1)..], ',', langIndex);
    }

    static Dictionary<string, string> ParseData(string data, char delimiter, int column = 1)
    {
        Dictionary<string, string> result = new();
        foreach (var line in data.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = line.Split(delimiter);
            if (parts.Length <= column) continue;
            var key = parts[0].Trim();
            if (!string.IsNullOrEmpty(key)) result[key] = parts[column].Trim();
        }

        return result;
    }

    public static async Task<Dictionary<string, string>> FetchGoogleSheetAsync(string sheetId, string range)
    {
        if (string.IsNullOrEmpty(_googleApiKey))
            throw new InvalidOperationException("Google API key not set. Call SetGoogleApiKey() first.");

        var url = $"https://sheets.googleapis.com/v4/spreadsheets/{sheetId}/values/{range}?key={_googleApiKey}";
        try
        {
            var response = await _client.GetStringAsync(url);
            var json = JsonUtility.FromJson<GoogleSheetResponse>(response);
            return json.values.ToDictionary(row => row[0], row => row.Length > 1 ? row[1] : "");
        }
        catch (HttpRequestException e)
        {
            DLog.LogE($"Network error while fetching Google Sheets data: {e.Message}");
        }
        catch (Exception e)
        {
            DLog.LogE($"Failed to parse Google Sheets data: {e.Message}");
        }

        return new Dictionary<string, string>();
    }

    [Serializable]
    class GoogleSheetResponse
    {
        public List<string[]> values;
    }
}