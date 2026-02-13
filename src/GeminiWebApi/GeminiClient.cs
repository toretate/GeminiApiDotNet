using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using GeminiWebApi.Constants;
using GeminiWebApi.Exceptions;
using GeminiWebApi.Models;
using GeminiWebApi.Utils;

namespace GeminiWebApi;

/// <summary>
/// Main Gemini API client for generating content and managing conversations.
/// </summary>
public class GeminiClient : IAsyncDisposable
{
    private readonly HttpClient _httpClient;
    private readonly string _securePsid;
    private readonly string _securePsidts;
    private readonly string? _proxy;
    private CookieContainer _cookieContainer;
    private int _requestId;
    private GemJar _gems;
    private bool _disposed;

    public string? SessionId { get; set; }
    public string? BuildLabel { get; set; }
    public GemJar Gems => _gems ??= new GemJar();

    /// <summary>
    /// Initialize a new instance of GeminiClient.
    /// </summary>
    public GeminiClient(string securePsid, string securePsidts, string? proxy = null)
    {
        _securePsid = securePsid ?? throw new ArgumentNullException(nameof(securePsid));
        _securePsidts = securePsidts;
        _proxy = proxy;
        _cookieContainer = new CookieContainer();
        _gems = new GemJar();
        _requestId = new Random().Next(10000, 99999);

        var handler = new HttpClientHandler
        {
            CookieContainer = _cookieContainer,
            AllowAutoRedirect = true
        };

        if (!string.IsNullOrEmpty(proxy))
        {
            handler.Proxy = new WebProxy(proxy);
            handler.UseProxy = true;
        }

        _httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        SetDefaultHeaders();
    }

    /// <summary>
    /// Set default HTTP headers for requests.
    /// </summary>
    private void SetDefaultHeaders()
    {
        _httpClient.DefaultRequestHeaders.Clear();

        foreach (var header in RequestHeaders.GeminiHeaders)
        {
            // Skip Content-Type as it should be set per request
            if (header.Key.Equals("Content-Type", StringComparison.OrdinalIgnoreCase))
                continue;
                
            _httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
        }

        AddCookies();
    }

    /// <summary>
    /// Add cookies to the request.
    /// </summary>
    private void AddCookies()
    {
        _cookieContainer.Add(new Uri(Endpoints.Init), new Cookie("__Secure-1PSID", _securePsid));
        if (!string.IsNullOrEmpty(_securePsidts))
        {
            _cookieContainer.Add(new Uri(Endpoints.Init), new Cookie("__Secure-1PSIDTS", _securePsidts));
        }
    }

    /// <summary>
    /// Initialize the client and fetch necessary metadata.
    /// </summary>
    public async Task InitializeAsync(
        int timeout = 30,
        bool autoClose = false,
        int closeDelay = 300,
        bool autoRefresh = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _httpClient.Timeout = TimeSpan.FromSeconds(timeout);

            // Fetch initial page to get session info
            var response = await _httpClient.GetAsync(Endpoints.Init, cancellationToken);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            // Extract session ID from response if available
            ExtractSessionInfo(content);

            Logger.Info("GeminiClient initialized successfully");
        }
        catch (Exception ex)
        {
            Logger.Error($"Initialization failed: {ex.Message}");
            throw new AuthenticationException("Failed to initialize GeminiClient: " + ex.Message, ex);
        }
    }

    /// <summary>
    /// Extract session information from page content.
    /// </summary>
    private void ExtractSessionInfo(string content)
    {
        // Look for session ID in the response
        var sessionMatch = System.Text.RegularExpressions.Regex.Match(
            content,
            @"""?sessionId""?\s*[:=]\s*[""']?([a-zA-Z0-9_-]+)[""']?"
        );

        if (sessionMatch.Success)
        {
            SessionId = sessionMatch.Groups[1].Value;
            Logger.Debug($"Session ID: {SessionId}");
        }
    }

    /// <summary>
    /// Generate content from a prompt.
    /// </summary>
    public async Task<ModelOutput> GenerateContentAsync(
        string prompt,
        List<string>? files = null,
        ModelType model = ModelType.Unspecified,
        string? gem = null,
        ChatSession? chat = null,
        CancellationToken cancellationToken = default)
    {
        var output = new ModelOutput();

        await foreach (var chunk in GenerateContentStreamAsync(
            prompt, files, model, gem, chat, cancellationToken))
        {
            output.Text += chunk.TextDelta;
            output.Thoughts += chunk.ThoughtsDelta;
            output.Images.AddRange(chunk.Images);
            output.Candidates = chunk.Candidates;
            output.Metadata = chunk.Metadata;

            if (!string.IsNullOrEmpty(chunk.Thoughts))
                output.Thoughts = chunk.Thoughts;
        }

        return output;
    }

    /// <summary>
    /// Generate content as a stream.
    /// </summary>
    public async IAsyncEnumerable<ModelOutput> GenerateContentStreamAsync(
        string prompt,
        List<string>? files = null,
        ModelType model = ModelType.Unspecified,
        string? gem = null,
        ChatSession? chat = null,
        [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var fileData = await PrepareFilesAsync(files);

        // Build the request
        var payload = BuildGeneratePayload(prompt, fileData, model, gem, chat);

        // Make the request
        var content = new StringContent(
            JsonUtils.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsync(Endpoints.Generate, content, cancellationToken);
        }
        catch (Exception ex)
        {
            Logger.Error($"Content generation failed: {ex.Message}");
            throw new ApiException("Content generation failed", ex);
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new ApiException($"API request failed: {response.StatusCode} - {errorContent}");
        }

        // Parse streaming response
        var streamContent = await response.Content.ReadAsStringAsync(cancellationToken);
        var frames = ResponseUtils.ParseStreamingResponse(streamContent);

        foreach (var frame in frames)
        {
            var output = ParseResponseFrame(frame);
            if (!string.IsNullOrEmpty(output.TextDelta) || !string.IsNullOrEmpty(output.ThoughtsDelta))
            {
                yield return output;
            }
        }
    }

    /// <summary>
    /// Prepare files for upload.
    /// </summary>
    private async Task<List<(string url, string name)>?> PrepareFilesAsync(List<string>? files)
    {
        if (files == null || files.Count == 0)
            return null;

        var uploadedFiles = new List<(string, string)>();

        foreach (var file in files)
        {
            try
            {
                var fileBytes = await File.ReadAllBytesAsync(file);
                var uploadUrl = await UploadFileAsync(fileBytes, Path.GetFileName(file));
                uploadedFiles.Add((uploadUrl, Path.GetFileName(file)));
            }
            catch (Exception ex)
            {
                Logger.Warning($"Failed to upload file {file}: {ex.Message}");
            }
        }

        return uploadedFiles;
    }

    /// <summary>
    /// Upload a file to Gemini storage.
    /// </summary>
    private async Task<string> UploadFileAsync(byte[] fileBytes, string fileName)
    {
        // This is a placeholder - in production, this would upload to Google's storage
        Logger.Debug($"Uploading file: {fileName}");

        // Return a mock URL for now
        return $"https://content-push.googleapis.com/upload/{Guid.NewGuid()}";
    }

    /// <summary>
    /// Build the payload for content generation.
    /// </summary>
    private object BuildGeneratePayload(
        string prompt,
        List<(string url, string name)>? fileData,
        ModelType model,
        string? gem,
        ChatSession? chat)
    {
        // Build message content structure
        var messageContent = new object[]
        {
            prompt,
            0,
            null,
            fileData,
            null,
            null,
            0
        };

        var reqId = (_requestId += 100000).ToString();
        var parameters = new Dictionary<string, object>
        {
            { "_reqid", reqId },
            { "rt", "c" }
        };

        if (!string.IsNullOrEmpty(BuildLabel))
            parameters["bl"] = BuildLabel;

        if (!string.IsNullOrEmpty(SessionId))
            parameters["sid"] = SessionId;

        // Get model information
        var modelInfo = Constants.Model.Models.TryGetValue(model, out var m)
            ? m
            : Constants.Model.Models[ModelType.Unspecified];

        return new
        {
            input = messageContent,
            parameters = parameters,
            model = modelInfo.ModelName,
            gem = gem,
            chat = new
            {
                conversationId = chat?.Metadata?.ConversationId,
                responseId = chat?.Metadata?.ResponseId
            }
        };
    }

    /// <summary>
    /// Parse a response frame.
    /// </summary>
    private ModelOutput ParseResponseFrame(JsonElement frame)
    {
        var output = new ModelOutput();

        try
        {
            // Extract text content
            var text = JsonUtils.GetNestedValue(frame, new[] { 1, 0 }, string.Empty);
            output.TextDelta = text?.ToString() ?? string.Empty;
            output.Text = output.TextDelta;

            // Extract images if present
            // This is a simplified version - full implementation would parse all image types
            if (frame.TryGetProperty("images", out var images))
            {
                // Parse images...
            }

            return output;
        }
        catch (Exception ex)
        {
            Logger.Debug($"Error parsing frame: {ex.Message}");
            return output;
        }
    }

    /// <summary>
    /// Start a new chat session.
    /// </summary>
    public ChatSession StartChat(
        ModelType model = ModelType.Unspecified,
        string? gem = null,
        ChatSessionMetadata? metadata = null)
    {
        ThrowIfDisposed();
        return new ChatSession(this, model, gem, metadata);
    }

    /// <summary>
    /// Batch execute multiple RPC calls.
    /// </summary>
    public async Task<HttpResponseMessage> BatchExecuteAsync(
        List<RpcData> rpcCalls,
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var payloads = string.Join(
            "&",
            rpcCalls.Select((call, idx) =>
                $"f.req={Uri.EscapeDataString($"[[\"{call.RpcId}\",\"{call.Payload}\",null,1]]")}&at={Uri.EscapeDataString(SessionId ?? "")}"
            )
        );

        var content = new StringContent(payloads, Encoding.UTF8, "application/x-www-form-urlencoded");
        var response = await _httpClient.PostAsync(Endpoints.BatchExecute, content, cancellationToken);

        return response;
    }

    /// <summary>
    /// Fetch available gems.
    /// </summary>
    public async Task<GemJar> FetchGemsAsync(
        bool includeHidden = false,
        string language = "en",
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        try
        {
            var rpcCalls = new List<RpcData>
            {
                new RpcData
                {
                    RpcId = GrpcRoutes.ListGems,
                    Payload = JsonUtils.Serialize(includeHidden ? new object[] { 4, new[] { language }, 0 } : new object[] { 3, new[] { language }, 0 }),
                    Identifier = "system"
                },
                new RpcData
                {
                    RpcId = GrpcRoutes.ListGems,
                    Payload = $"[2,[\"{language}\"],0]",
                    Identifier = "custom"
                }
            };

            var response = await BatchExecuteAsync(rpcCalls, cancellationToken);
            var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

            ParseGemsResponse(responseText);
            return _gems;
        }
        catch (Exception ex)
        {
            Logger.Error($"Failed to fetch gems: {ex.Message}");
            throw new ApiException("Failed to fetch gems: " + ex.Message, ex);
        }
    }

    /// <summary>
    /// Parse gems response.
    /// </summary>
    private void ParseGemsResponse(string responseText)
    {
        // Parse the response and populate _gems
        // This is a simplified version
        try
        {
            var frames = ResponseUtils.ParseStreamingResponse(responseText);
            // Parse gem data from frames...
        }
        catch (Exception ex)
        {
            Logger.Debug($"Error parsing gems response: {ex.Message}");
        }
    }

    /// <summary>
    /// Create a new custom gem.
    /// </summary>
    public async Task<Gem> CreateGemAsync(
        string name,
        string prompt,
        string description = "",
        CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();

        var gemData = new object[]
        {
            name,
            description,
            prompt,
            null, null, null, null, null, 0, null, 1, null, null, null, new object[0]
        };

        var rpcCall = new RpcData
        {
            RpcId = GrpcRoutes.CreateGem,
            Payload = JsonUtils.Serialize(new[] { gemData })
        };

        var response = await BatchExecuteAsync(new List<RpcData> { rpcCall }, cancellationToken);
        var responseText = await response.Content.ReadAsStringAsync(cancellationToken);

        // Parse response and return created gem
        return new Gem { Name = name, Prompt = prompt, Description = description };
    }

    /// <summary>
    /// Close the client and cleanup resources.
    /// </summary>
    public async Task CloseAsync()
    {
        if (_disposed)
            return;

        _disposed = true;
        _httpClient?.Dispose();
        await Task.CompletedTask;
    }

    private void ThrowIfDisposed()
    {
        if (_disposed)
            throw new ObjectDisposedException(nameof(GeminiClient));
    }

    public async ValueTask DisposeAsync()
    {
        await CloseAsync();
        GC.SuppressFinalize(this);
    }

    public void Dispose()
    {
        CloseAsync().Wait();
    }
}

/// <summary>
/// Represents a chat session with conversation history.
/// </summary>
public class ChatSession
{
    private readonly GeminiClient _client;
    private readonly ModelType _model;
    private readonly string? _gem;

    public ChatSessionMetadata Metadata { get; set; }

    public ChatSession(
        GeminiClient client,
        ModelType model = ModelType.Unspecified,
        string? gem = null,
        ChatSessionMetadata? metadata = null)
    {
        _client = client;
        _model = model;
        _gem = gem;
        Metadata = metadata ?? new ChatSessionMetadata();
    }

    /// <summary>
    /// Send a message in the chat.
    /// </summary>
    public async Task<ModelOutput> SendMessageAsync(
        string message,
        List<string>? files = null,
        CancellationToken cancellationToken = default)
    {
        return await _client.GenerateContentAsync(message, files, _model, _gem, this, cancellationToken);
    }

    /// <summary>
    /// Send a message as a stream.
    /// </summary>
    public async IAsyncEnumerable<ModelOutput> SendMessageStreamAsync(
        string message,
        List<string>? files = null)
    {
        await foreach (var chunk in _client.GenerateContentStreamAsync(message, files, _model, _gem, this))
        {
            yield return chunk;
        }
    }

    /// <summary>
    /// Choose a candidate response.
    /// </summary>
    public void ChooseCandidate(int index)
    {
        Metadata.ChosenIndex = index;
    }
}

// Helper class for JSON serialization
internal static class Json
{
    public static string Serialize<T>(T obj)
    {
        return JsonUtils.Serialize(obj);
    }
}
