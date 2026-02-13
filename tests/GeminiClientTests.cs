using System;
using Xunit;
using GeminiWebApi;
using GeminiWebApi.Constants;
using GeminiWebApi.Exceptions;
using GeminiWebApi.Models;
using GeminiWebApi.Utils;

namespace GeminiWebApi.Tests;

public class GeminiClientTests
{
    [Fact]
    public void TestClientInitialization()
    {
        var client = new GeminiClient("test_psid", "test_psidts");
        Assert.NotNull(client);
        Assert.Empty(client.Gems);
    }

    [Fact]
    public void TestClientDisposal()
    {
        var client = new GeminiClient("test_psid", "test_psidts");
        client.Dispose();
        Assert.Throws<ObjectDisposedException>(() => client.StartChat());
    }
}

public class ModelOutputTests
{
    [Fact]
    public void TestModelOutputCreation()
    {
        var output = new ModelOutput
        {
            Text = "Test response",
            TextDelta = "delta",
            Thoughts = "thinking"
        };

        Assert.Equal("Test response", output.Text);
        Assert.Equal("delta", output.TextDelta);
        Assert.Equal("thinking", output.Thoughts);
    }

    [Fact]
    public void TestModelOutputToString()
    {
        var output = new ModelOutput { Text = "Hello" };
        Assert.Equal("Hello", output.ToString());
    }
}

public class GemTests
{
    [Fact]
    public void TestGemCreation()
    {
        var gem = new Gem
        {
            Id = "test-gem",
            Name = "Test Gem",
            Prompt = "This is a test gem"
        };

        Assert.Equal("test-gem", gem.Id);
        Assert.Equal("Test Gem", gem.Name);
    }

    [Fact]
    public void TestGemJarFiltering()
    {
        var jar = new GemJar
        {
            new Gem { Id = "1", Name = "Gem1", IsPredefined = true },
            new Gem { Id = "2", Name = "Gem2", IsPredefined = false },
            new Gem { Id = "3", Name = "Gem3", IsPredefined = true }
        };

        var predefined = jar.Filter(predefined: true);
        Assert.Equal(2, predefined.Count);

        var custom = jar.Filter(predefined: false);
        Assert.Single(custom);
    }

    [Fact]
    public void TestGemJarGet()
    {
        var jar = new GemJar
        {
            new Gem { Id = "test-id", Name = "Test Gem" }
        };

        var byId = jar.Get(id: "test-id");
        Assert.NotNull(byId);
        Assert.Equal("Test Gem", byId.Name);

        var byName = jar.Get(name: "Test Gem");
        Assert.NotNull(byName);
        Assert.Equal("test-id", byName.Id);
    }
}

public class ImageTests
{
    [Fact]
    public void TestWebImageInitialization()
    {
        var image = new WebImage
        {
            Url = "https://example.com/image.png",
            Alt = "Test image",
            Title = "Test Title"
        };

        Assert.Equal("https://example.com/image.png", image.Url);
        Assert.Equal("Test image", image.Alt);
        Assert.Equal("Test Title", image.Title);
    }

    [Fact]
    public void TestGeneratedImageInitialization()
    {
        var image = new GeneratedImage
        {
            Data = "base64encodeddata",
            Alt = "Generated image"
        };

        Assert.Equal("base64encodeddata", image.Data);
        Assert.Equal("Generated image", image.Alt);
    }
}

public class TextUtilsTests
{
    [Fact]
    public void TestGetCleanText()
    {
        var text = "Hello world\\```";
        var cleaned = TextUtils.GetCleanText(text);
        Assert.NotNull(cleaned);
    }

    [Fact]
    public void TestGetDeltaByText()
    {
        var (delta, full) = TextUtils.GetDeltaByText("Hello world", "Hello ", false);
        Assert.NotEmpty(delta);
        Assert.Equal("Hello world", full);
    }
}

public class ConstantsTests
{
    [Fact]
    public void TestEndpointsAreSet()
    {
        Assert.Equal("https://www.google.com", Endpoints.Google);
        Assert.Equal("https://gemini.google.com/app", Endpoints.Init);
    }

    [Fact]
    public void TestGrpcRoutesAreSet()
    {
        Assert.NotEmpty(GrpcRoutes.ListGems);
        Assert.NotEmpty(GrpcRoutes.CreateGem);
    }

    [Fact]
    public void TestModelDefinitions()
    {
        Assert.NotNull(Constants.Model.Models);
        Assert.NotEmpty(Constants.Model.Models);
        
        var unspeModel = Constants.Model.Models[ModelType.Unspecified];
        Assert.Equal("unspecified", unspeModel.ModelName);
    }
}

public class ExceptionTests
{
    [Fact]
    public void TestAuthenticationException()
    {
        var ex = new AuthenticationException("Test auth error");
        Assert.Equal("Test auth error", ex.Message);
    }

    [Fact]
    public void TestModelInvalidException()
    {
        var ex = new ModelInvalidException("Invalid model");
        Assert.Equal("Invalid model", ex.Message);
    }

    [Fact]
    public void TestUsageLimitExceededException()
    {
        var ex = new UsageLimitExceededException("Limit exceeded");
        Assert.Equal("Limit exceeded", ex.Message);
    }

    [Fact]
    public void TestGeminiExceptionHierarchy()
    {
        var exception = new AuthenticationException("Auth failed");
        Assert.IsAssignableFrom<GeminiException>(exception);
    }
}

public class ChatSessionMetadataTests
{
    [Fact]
    public void TestMetadataCreation()
    {
        var metadata = new ChatSessionMetadata
        {
            ConversationId = "conv123",
            ResponseId = "resp456",
            ChosenIndex = 0
        };

        Assert.Equal("conv123", metadata.ConversationId);
        Assert.Equal("resp456", metadata.ResponseId);
        Assert.Equal(0, metadata.ChosenIndex);
    }

    [Fact]
    public void TestMetadataToString()
    {
        var metadata = new ChatSessionMetadata { ConversationId = "test-conv" };
        Assert.Contains("test-conv", metadata.ToString());
    }
}
