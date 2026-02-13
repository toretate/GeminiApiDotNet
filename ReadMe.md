# Gemini Api - C# Gemini API Wrapper

<img src="https://raw.githubusercontent.com/HanaokaYuzu/Gemini-API/master/assets/logo.svg" width="35px" alt="Gemini Icon" /> 

<div align="center">

### Language / 言語

[🇯🇵 日本語](ReadMe.ja.md) | [🇬🇧 English](ReadMe.md) | [🌐 Google Translate](https://translate.google.com/translate?sl=en&tl=ja&u=https://github.com/toretate/GeminiApiDotNet/blob/main/ReadMe.md)

</div>

C# reverse-engineered wrapper for [Google Gemini](https://gemini.google.com/) web app.

This project aims to create a C# version of the Python project below:
https://github.com/HanaokaYuzu/Gemini-API

**Base Version:**
- **Repository**: https://github.com/HanaokaYuzu/Gemini-API
- **Commit**: `3f2b935420fd688e1ab84d937de2c33a871697c0`
- **Date**: 2026-02-10
- **Message**: Fix the retry logic for unsaved conversations. (#233)

## Overview

A C# port of the original [Python Gemini API](https://github.com/HanaokaYuzu/Gemini-API) project. Provides an async-first wrapper for the Google Gemini Web API with the following features:

- **Persistent Cookies** - Automatic cookie management and updates
- **Image Generation** - Support for image generation and editing
- **System Prompts** - Customizable prompts using Gems
- **Extensions Support** - Integration with Gemini extensions
- **Streaming Mode** - Real-time content generation
- **Multi-turn Conversations** - Built-in chat session management
- **Async** - Full .NET async/await support

## Requirements

- .NET 8.0 or higher
- C# 11 or higher
- Valid Google account with Gemini access

## Installation

### Building from Source

```bash
git clone <repository-url>
cd GeminiWebApiDotnet
dotnet build
```

## Authentication

1. Access [https://gemini.google.com](https://gemini.google.com/) and log in
2. Open Developer Tools (F12) and navigate to the Network tab
3. Refresh the page
4. Click on a request and copy the following cookie values:
   - `__Secure-1PSID`
   - `__Secure-1PSIDTS` (optional)

## Quick Start

### Basic Usage

```csharp
using GeminiWebApi;
using GeminiWebApi.Utils;

Logger.SetLogLevel(Logger.LogLevel.Info);

var client = new GeminiClient(
    securePsid: "YOUR_SECURE_1PSID",
    securePsidts: "YOUR_SECURE_1PSIDTS"
);

await client.InitializeAsync(timeout: 30);

var response = await client.GenerateContentAsync("Hello World!");
Console.WriteLine(response.Text);

await client.CloseAsync();
```

### Streaming

```csharp
await foreach (var chunk in client.GenerateContentStreamAsync("Tell me a story"))
{
    Console.Write(chunk.TextDelta);
}
```

### Multi-turn Conversation

```csharp
var chat = client.StartChat();

var response1 = await chat.SendMessageAsync("What is machine learning?");
Console.WriteLine(response1.Text);

var response2 = await chat.SendMessageAsync("Can you give me an example?");
Console.WriteLine(response2.Text);
```

### Model Selection

```csharp
using GeminiWebApi.Constants;

var response = await client.GenerateContentAsync(
    "What model are you?",
    model: ModelType.Gemini30Flash
);
```

Available Models:
- `ModelType.Unspecified` - Default model
- `ModelType.Gemini30Pro` - Gemini 3.0 Pro
- `ModelType.Gemini30Flash` - Gemini 3.0 Flash  
- `ModelType.Gemini30FlashThinking` - Gemini 3.0 Flash Thinking

### Gems Panel (System Prompts)

```csharp
// Fetch available Gems
var gems = await client.FetchGemsAsync(includeHidden: false);

// Search for a specific Gem
var codingGem = gems.Get(name: "Coding Helper");

// Create a custom Gem
var customGem = await client.CreateGemAsync(
    name: "Python Expert",
    prompt: "You are an expert Python developer",
    description: "Helps with Python programming"
);

// Use Gem in conversation
var chat = client.StartChat(gem: customGem.Id);
var response = await chat.SendMessageAsync("How do I use decorators?");
```

### Image Processing

```csharp
// Generate an image
var response = await client.GenerateContentAsync("Generate an image of a sunset");

// Access images in the response
foreach (var image in response.Images)
{
    Console.WriteLine($"Image: {image.Title}");
    await image.SaveAsync(path: "./images/", filename: "sunset.png");
}
```

## Error Handling

```csharp
using GeminiWebApi.Exceptions;

try
{
    var response = await client.GenerateContentAsync("Hello");
}
catch (AuthenticationException ex)
{
    Console.WriteLine($"Authentication failed: {ex.Message}");
}
catch (ModelInvalidException ex)
{
    Console.WriteLine($"Invalid model: {ex.Message}");
}
catch (GeminiException ex)
{
    Console.WriteLine($"Error: {ex.Message}");
}
```

## Logging

Control logging output:

```csharp
using GeminiWebApi.Utils;

Logger.SetLogLevel(Logger.LogLevel.Debug);      // Verbose logging
Logger.SetLogLevel(Logger.LogLevel.Info);       // Info level
Logger.SetLogLevel(Logger.LogLevel.Warning);    // Warnings only
Logger.SetLogLevel(Logger.LogLevel.Error);      // Errors only
```

## API Reference

### GeminiClient

The main class for interacting with the Gemini API.

**Methods:**
- `InitializeAsync()` - Initialize the client
- `GenerateContentAsync()` - Generate content from a prompt
- `GenerateContentStreamAsync()` - Generate content with streaming
- `StartChat()` - Create a new chat session
- `FetchGemsAsync()` - Retrieve available Gems
- `CreateGemAsync()` - Create a custom Gem
- `CloseAsync()` - Close and clean up the client

### ChatSession

Represents a conversation session.

**Methods:**
- `SendMessageAsync()` - Send a message in the chat
- `SendMessageStreamAsync()` - Send a message with streaming
- `ChooseCandidate()` - Select a specific response candidate

### ModelOutput

The result of content generation.

**Properties:**
- `Text` - The complete generated text
- `TextDelta` - New text since the last chunk (for streaming)
- `Thoughts` - Model's thinking process (when available)
- `Images` - List of images in the response

## Project Structure

```
GeminiWebApiDotnet/
├── src/GeminiWebApi/
│   ├── Constants/          # API constants and endpoints
│   ├── Exceptions/         # Custom exceptions
│   ├── Models/             # Data models
│   ├── Utils/              # Utility functions
│   └── GeminiClient.cs     # Main client class
├── example/                 # Sample usage
├── tests/                   # Unit tests
├── GeminiWebApi.csproj     # Project file
└── ReadMe.md
```

## License

AGPL-3.0 License - See the LICENSE file for details

## Note

This project is a reverse-engineered implementation and is not affiliated with Google. Use at your own risk and in compliance with Google's Terms of Service.

## See Also

For Japanese documentation, see [ReadMe.ja.md](ReadMe.ja.md)