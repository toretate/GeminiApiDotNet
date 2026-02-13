using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GeminiWebApi.Models;

/// <summary>
/// Represents an image in the response.
/// </summary>
public abstract class Image
{
    [JsonPropertyName("alt")]
    public string? Alt { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    public abstract Task SaveAsync(string path = "./", string? filename = null, bool skipInvalidFilename = true);
}

/// <summary>
/// Represents a web image.
/// </summary>
public class WebImage : Image
{
    public override async Task SaveAsync(string path = "./", string? filename = null, bool skipInvalidFilename = true)
    {
        if (string.IsNullOrEmpty(Url))
            throw new InvalidOperationException("Image URL is not available");

        using var client = new HttpClient();
        var imageData = await client.GetByteArrayAsync(Url);

        var directory = Path.GetDirectoryName(path) ?? "./";
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        var fileName = filename ?? Path.GetFileName(Url);
        if (string.IsNullOrEmpty(fileName) && skipInvalidFilename)
            return;

        var filePath = Path.Combine(path, fileName ?? "image.png");
        await File.WriteAllBytesAsync(filePath, imageData);
    }
}

/// <summary>
/// Represents a generated image.
/// </summary>
public class GeneratedImage : Image
{
    [JsonPropertyName("data")]
    public string? Data { get; set; }

    public override async Task SaveAsync(string path = "./", string? filename = null, bool skipInvalidFilename = true)
    {
        if (string.IsNullOrEmpty(Data))
            throw new InvalidOperationException("Image data is not available");

        var directory = Path.GetDirectoryName(path) ?? "./";
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        var fileName = filename ?? $"generated_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        var filePath = Path.Combine(path, fileName);

        // Data should be base64 encoded
        var imageData = Convert.FromBase64String(Data);
        await File.WriteAllBytesAsync(filePath, imageData);
    }
}

/// <summary>
/// Represents a candidate response.
/// </summary>
public class Candidate
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("finishReason")]
    public string? FinishReason { get; set; }

    public override string ToString() => Content ?? string.Empty;
}

/// <summary>
/// Represents the model output/response.
/// </summary>
public class ModelOutput
{
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("textDelta")]
    public string TextDelta { get; set; } = string.Empty;

    [JsonPropertyName("thoughts")]
    public string Thoughts { get; set; } = string.Empty;

    [JsonPropertyName("thoughtsDelta")]
    public string ThoughtsDelta { get; set; } = string.Empty;

    [JsonPropertyName("images")]
    public List<Image> Images { get; set; } = new();

    [JsonPropertyName("candidates")]
    public List<Candidate> Candidates { get; set; } = new();

    [JsonPropertyName("metadata")]
    public Dictionary<string, object>? Metadata { get; set; }

    public override string ToString() => Text;
}

/// <summary>
/// Represents a Gem (custom system prompt).
/// </summary>
public class Gem
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("prompt")]
    public string Prompt { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("predefined")]
    public bool IsPredefined { get; set; }

    public override string ToString() => Name;
}

/// <summary>
/// Collection of gems with filtering capabilities.
/// </summary>
public class GemJar : List<Gem>
{
    public Gem? Get(string? id = null, string? name = null)
    {
        if (!string.IsNullOrEmpty(id))
            return this.FirstOrDefault(g => g.Id == id);
        if (!string.IsNullOrEmpty(name))
            return this.FirstOrDefault(g => g.Name == name);
        return null;
    }

    public GemJar Filter(bool? predefined = null)
    {
        var result = new GemJar();
        var filtered = predefined.HasValue
            ? this.Where(g => g.IsPredefined == predefined.Value)
            : this.AsEnumerable();

        foreach (var gem in filtered)
            result.Add(gem);

        return result;
    }
}

/// <summary>
/// Chat session metadata.
/// </summary>
public class ChatSessionMetadata
{
    [JsonPropertyName("conversationId")]
    public string? ConversationId { get; set; }

    [JsonPropertyName("responseId")]
    public string? ResponseId { get; set; }

    [JsonPropertyName("chosenIndex")]
    public int ChosenIndex { get; set; }

    public override string ToString() => $"CID: {ConversationId}";
}

/// <summary>
/// Data for RPC calls.
/// </summary>
public class RpcData
{
    public string RpcId { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public string? Identifier { get; set; }
}
