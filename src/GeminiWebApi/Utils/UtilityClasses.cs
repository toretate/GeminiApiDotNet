using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace GeminiWebApi.Utils;

/// <summary>
/// Utility methods for JSON parsing and manipulation.
/// </summary>
public static class JsonUtils
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    /// <summary>
    /// Get a nested value from a JSON object using a path.
    /// </summary>
    public static T? GetNestedValue<T>(JsonElement element, int[] path, T? defaultValue = default)
    {
        JsonElement current = element;

        foreach (int index in path)
        {
            if (current.ValueKind != JsonValueKind.Array || index >= current.GetArrayLength())
                return defaultValue;

            current = current[index];
        }

        try
        {
            return current.Deserialize<T>(DefaultOptions) ?? defaultValue;
        }
        catch
        {
            return defaultValue;
        }
    }

    /// <summary>
    /// Get a nested value from a dictionary using a path.
    /// </summary>
    public static object? GetNestedValue(object? obj, string[] path, object? defaultValue = null)
    {
        if (obj == null)
            return defaultValue;

        foreach (string key in path)
        {
            if (obj is Dictionary<string, object> dict)
            {
                obj = dict.TryGetValue(key, out var value) ? value : null;
            }
            else if (obj is JsonElement jsonElement)
            {
                if (!jsonElement.TryGetProperty(key, out var prop))
                    return defaultValue;
                obj = prop;
            }
            else
            {
                return defaultValue;
            }

            if (obj == null)
                return defaultValue;
        }

        return obj ?? defaultValue;
    }

    /// <summary>
    /// Serialize an object to JSON string.
    /// </summary>
    public static string Serialize<T>(T obj)
    {
        return JsonSerializer.Serialize(obj, DefaultOptions);
    }

    /// <summary>
    /// Deserialize a JSON string.
    /// </summary>
    public static T? Deserialize<T>(string json)
    {
        return JsonSerializer.Deserialize<T>(json, DefaultOptions);
    }
}

/// <summary>
/// Utility methods for text parsing.
/// </summary>
public static class TextUtils
{
    private static readonly HashSet<char> VolatileChars = new(" \t\n\r!@#$%^&*()-_=+[]{};:'\",.<>?/\\|`~");

    /// <summary>
    /// Clean Gemini text by removing trailing code block artifacts.
    /// </summary>
    public static string GetCleanText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        // Remove trailing code block markers
        var cleaned = System.Text.RegularExpressions.Regex.Replace(text, @"\\+[`*_~].*$", "");
        return cleaned;
    }

    /// <summary>
    /// Calculate text delta between old and new text.
    /// </summary>
    public static (string delta, string fullText) GetDeltaByText(string newRaw, string lastSent, bool isFinal)
    {
        var newClean = isFinal ? newRaw : GetCleanText(newRaw);

        if (newClean.StartsWith(lastSent))
        {
            return (newClean[lastSent.Length..], newClean);
        }

        // Simple fallback: return the difference
        int commonLength = 0;
        for (int i = 0; i < Math.Min(newClean.Length, lastSent.Length); i++)
        {
            if (newClean[i] == lastSent[i])
                commonLength++;
            else
                break;
        }

        return (newClean[commonLength..], newClean);
    }
}

/// <summary>
/// Logger utility.
/// </summary>
public static class Logger
{
    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Critical
    }

    private static LogLevel _currentLevel = LogLevel.Info;

    public static void SetLogLevel(LogLevel level)
    {
        _currentLevel = level;
    }

    public static void Debug(string message)
    {
        if (_currentLevel <= LogLevel.Debug)
            Console.WriteLine($"[DEBUG] {message}");
    }

    public static void Info(string message)
    {
        if (_currentLevel <= LogLevel.Info)
            Console.WriteLine($"[INFO] {message}");
    }

    public static void Warning(string message)
    {
        if (_currentLevel <= LogLevel.Warning)
            Console.WriteLine($"[WARNING] {message}");
    }

    public static void Error(string message)
    {
        if (_currentLevel <= LogLevel.Error)
            Console.WriteLine($"[ERROR] {message}");
    }

    public static void Critical(string message)
    {
        Console.WriteLine($"[CRITICAL] {message}");
    }
}

/// <summary>
/// Cookie utility for handling authentication.
/// </summary>
public static class CookieUtils
{
    /// <summary>
    /// Parse cookies from a response header.
    /// </summary>
    public static Dictionary<string, string> ParseCookies(string cookieString)
    {
        var cookies = new Dictionary<string, string>();
        if (string.IsNullOrEmpty(cookieString))
            return cookies;

        foreach (var cookie in cookieString.Split(';'))
        {
            var parts = cookie.Split('=', 2);
            if (parts.Length == 2)
            {
                cookies[parts[0].Trim()] = parts[1].Trim();
            }
        }

        return cookies;
    }

    /// <summary>
    /// Format cookies for HTTP request.
    /// </summary>
    public static string FormatCookies(Dictionary<string, string> cookies)
    {
        return string.Join("; ", cookies.Select(kv => $"{kv.Key}={kv.Value}"));
    }
}

/// <summary>
/// Response parsing utility.
/// </summary>
public static class ResponseUtils
{
    /// <summary>
    /// Extract JSON from a streaming response.
    /// </summary>
    public static List<JsonElement> ParseStreamingResponse(string content)
    {
        var frames = new List<JsonElement>();

        if (string.IsNullOrEmpty(content))
            return frames;

        var lines = content.Split('\n');
        for (int i = 0; i < lines.Length - 1; i++)
        {
            if (int.TryParse(lines[i], out int length) && i + 1 < lines.Length)
            {
                try
                {
                    var jsonStr = lines[i + 1];
                    var doc = JsonDocument.Parse(jsonStr);
                    frames.Add(doc.RootElement.Clone());
                    i++; // Skip the JSON line
                }
                catch
                {
                    // Skip invalid JSON
                }
            }
        }

        return frames;
    }
}

/// <summary>
/// File upload utilities.
/// </summary>
public static class FileUtils
{
    /// <summary>
    /// Parse file name from various sources.
    /// </summary>
    public static string ParseFileName(string filePath)
    {
        return Path.GetFileName(filePath);
    }

    /// <summary>
    /// Generate a random filename.
    /// </summary>
    public static string GenerateRandomFileName(string extension = ".txt")
    {
        var randomName = new Random().Next(1000000000, int.MaxValue);
        return randomName + extension;
    }
}
