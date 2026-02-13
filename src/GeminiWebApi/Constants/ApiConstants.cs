using System;
using System.Collections.Generic;

namespace GeminiWebApi.Constants;

/// <summary>
/// Contains API endpoints used by Gemini Web API.
/// </summary>
public static class Endpoints
{
    public const string Google = "https://www.google.com";
    public const string Init = "https://gemini.google.com/app";
    public const string Generate = "https://gemini.google.com/_/BardChatUi/data/assistant.lamda.BardFrontendService/StreamGenerate";
    public const string RotateCookies = "https://accounts.google.com/RotateCookies";
    public const string Upload = "https://content-push.googleapis.com/upload";
    public const string BatchExecute = "https://gemini.google.com/_/BardChatUi/data/batchexecute";
}

/// <summary>
/// Google RPC IDs used in Gemini API.
/// </summary>
public static class GrpcRoutes
{
    // Chat methods
    public const string ListChats = "MaZiqc";
    public const string ReadChat = "hNvQHb";
    public const string DeleteChat = "GzXR5e";

    // Gem methods
    public const string ListGems = "CNgdBe";
    public const string CreateGem = "oMH3Zd";
    public const string UpdateGem = "kHv0Vd";
    public const string DeleteGem = "UXcSJb";

    // Activity methods
    public const string BardActivity = "ESY5D";
}

/// <summary>
/// HTTP Headers used in API requests.
/// </summary>
public static class RequestHeaders
{
    public static readonly Dictionary<string, string> GeminiHeaders = new()
    {
        { "Content-Type", "application/x-www-form-urlencoded;charset=utf-8" },
        { "Host", "gemini.google.com" },
        { "Origin", "https://gemini.google.com" },
        { "Referer", "https://gemini.google.com/" },
        { "User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/144.0.0.0 Safari/537.36" },
        { "X-Same-Domain", "1" }
    };

    public static readonly Dictionary<string, string> RotateCookiesHeaders = new()
    {
        { "Content-Type", "application/json" }
    };

    public static readonly Dictionary<string, string> UploadHeaders = new()
    {
        { "Push-ID", "feeds/mcudyrk2a4khkz" }
    };
}

/// <summary>
/// Supported language models.
/// </summary>
public enum ModelType
{
    Unspecified,
    Gemini30Pro,
    Gemini30Flash,
    Gemini30FlashThinking
}

/// <summary>
/// Model definitions with names and headers.
/// </summary>
public class Model
{
    public string ModelName { get; set; } = string.Empty;
    public Dictionary<string, string> Headers { get; set; } = new();
    public bool RequiresSpecialHandling { get; set; }

    public static readonly Dictionary<ModelType, Model> Models = new()
    {
        { ModelType.Unspecified, new Model { ModelName = "unspecified", Headers = new() } },
        { ModelType.Gemini30Pro, new Model 
        { 
            ModelName = "gemini-3.0-pro",
            Headers = new() { { "x-goog-ext-525001261-jspb", "[1,null,null,null,\"9d8ca3786ebdfbea\",null,null,0,[4],null,null,1]" } }
        }},
        { ModelType.Gemini30Flash, new Model 
        { 
            ModelName = "gemini-3.0-flash",
            Headers = new() { { "x-goog-ext-525001261-jspb", "[1,null,null,null,\"fbb127bbb056c959\",null,null,0,[4],null,null,1]" } }
        }},
        { ModelType.Gemini30FlashThinking, new Model 
        { 
            ModelName = "gemini-3.0-flash-thinking",
            Headers = new() { { "x-goog-ext-525001261-jspb", "[1,null,null,null,\"5bf011840784117a\",null,null,0,[4],null,null,1]" } }
        }}
    };
}

/// <summary>
/// Error codes returned from the server.
/// </summary>
public enum ErrorCode
{
    TemporaryError1013 = 1013,      // Randomly raised when generating with certain models
    UsageLimitExceeded = 1037,
    ModelInconsistent = 1050,
    ModelHeaderInvalid = 1052,
    IpTemporarilyBlocked = 1060
}
