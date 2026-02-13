using System;
using System.Threading.Tasks;
using GeminiWebApi;
using GeminiWebApi.Constants;
using GeminiWebApi.Utils;

namespace GeminiWebApi.Examples;

class Program
{
    static async Task Main(string[] args)
    {
        // Set log level
        Logger.SetLogLevel(Logger.LogLevel.Debug);

        // Your Gemini cookies
        const string securePsid = "YOUR_SECURE_1PSID_HERE";
        const string securePsidts = "YOUR_SECURE_1PSIDTS_HERE";

        // Initialize client
        using var client = new GeminiClient(securePsid, securePsidts, proxy: null);

        try
        {
            await client.InitializeAsync(timeout: 30);
            Console.WriteLine("✓ Client initialized successfully\n");

            // Example 1: Simple content generation
            Console.WriteLine("=== Example 1: Simple Content Generation ===");
            var response = await client.GenerateContentAsync("Hello World! What is 2+2?");
            Console.WriteLine($"Response: {response.Text}\n");

            // Example 2: Streaming content generation
            Console.WriteLine("=== Example 2: Streaming Content ===");
            await foreach (var chunk in client.GenerateContentStreamAsync(
                "Write a short poem about programming"))
            {
                Console.Write(chunk.TextDelta);
            }
            Console.WriteLine("\n");

            // Example 3: Multi-turn conversation
            Console.WriteLine("=== Example 3: Multi-turn Conversation ===");
            var chat = client.StartChat();

            var response1 = await chat.SendMessageAsync("Tell me about C#");
            Console.WriteLine($"Assistant: {response1.Text}\n");

            var response2 = await chat.SendMessageAsync("What are its main features?");
            Console.WriteLine($"Assistant: {response2.Text}\n");

            // Example 4: Model selection
            Console.WriteLine("=== Example 4: Model Selection ===");
            var response3 = await client.GenerateContentAsync(
                "What model are you using?",
                model: ModelType.Gemini30Flash
            );
            Console.WriteLine($"Response with Gemini 3.0 Flash: {response3.Text}\n");

            // Example 5: Fetch gems
            Console.WriteLine("=== Example 5: Fetch Gems ===");
            var gems = await client.FetchGemsAsync(includeHidden: false, language: "en");
            Console.WriteLine($"Available gems: {gems.Count}");
            foreach (var gem in gems)
            {
                Console.WriteLine($"  - {gem.Name} (ID: {gem.Id})");
            }
            Console.WriteLine();

            // Example 6: Create custom gem
            Console.WriteLine("=== Example 6: Create Custom Gem ===");
            var customGem = await client.CreateGemAsync(
                name: "C# Expert",
                prompt: "You are an expert in C# programming and provide detailed explanations.",
                description: "A specialized gem for C# programming help"
            );
            Console.WriteLine($"✓ Created gem: {customGem.Name}\n");

            // Example 7: Use gem in conversation
            Console.WriteLine("=== Example 7: Using Gem ===");
            var chatWithGem = client.StartChat(gem: customGem.Id);
            var response4 = await chatWithGem.SendMessageAsync("Explain async/await in C#");
            Console.WriteLine($"Response with gem: {response4.Text}\n");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"✗ Error: {ex.Message}");
            if (ex.InnerException != null)
                Console.WriteLine($"  Inner: {ex.InnerException.Message}");
        }
        finally
        {
            await client.CloseAsync();
            Console.WriteLine("✓ Client closed");
        }
    }
}
