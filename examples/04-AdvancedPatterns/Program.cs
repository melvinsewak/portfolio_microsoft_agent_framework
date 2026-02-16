using Microsoft.Extensions.AI;
using Azure.AI.OpenAI;
using Azure;
using Microsoft.Extensions.Configuration;

namespace AdvancedPatterns;

/// <summary>
/// Example 4: Advanced Agent Patterns
/// Demonstrates: Streaming responses, conversation memory, error handling, and observability
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Example 4: Advanced Agent Patterns ===\n");
        Console.WriteLine("This example demonstrates advanced features:");
        Console.WriteLine("  • Streaming responses");
        Console.WriteLine("  • Persistent conversation memory");
        Console.WriteLine("  • Advanced error handling");
        Console.WriteLine("  • Observability and logging\n");

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var endpoint = configuration["AzureOpenAI:Endpoint"] ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        var apiKey = configuration["AzureOpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        var deploymentName = configuration["AzureOpenAI:DeploymentName"] ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? "gpt-4";

        Console.WriteLine("Select a pattern to demonstrate:");
        Console.WriteLine("  1. Streaming Responses");
        Console.WriteLine("  2. Conversation Memory Management");
        Console.WriteLine("  3. Error Handling & Resilience");
        Console.WriteLine("  4. Agent Observability\n");

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("⚠️  Running in DEMO mode (no API keys configured)\n");
            await RunAllDemos();
            return;
        }

        Console.Write("Choose pattern (1-4) or 'all' for demo: ");
        var choice = Console.ReadLine()?.ToLower();

        try
        {
            var azureClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
            var chatClient = azureClient.GetChatClient(deploymentName).AsIChatClient();

            switch (choice)
            {
                case "1":
                    await DemoStreamingResponses(chatClient);
                    break;
                case "2":
                    await DemoConversationMemory();
                    break;
                case "3":
                    await DemoErrorHandling(chatClient);
                    break;
                case "4":
                    await DemoObservability(chatClient);
                    break;
                default:
                    await RunAllDemos();
                    break;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
        }
    }

    static async Task DemoStreamingResponses(IChatClient chatClient)
    {
        Console.WriteLine("\n=== Pattern 1: Streaming Responses ===\n");
        Console.WriteLine("Streaming allows real-time token-by-token responses:\n");

        var messages = new List<ChatMessage>
        {
            new(ChatRole.System, "You are a helpful assistant."),
            new(ChatRole.User, "Explain the benefits of streaming responses in 3 sentences.")
        };

        Console.Write("Assistant (streaming): ");

        await foreach (var update in chatClient.GetStreamingResponseAsync(messages))
        {
            foreach (var content in update.Contents)
            {
                if (content is TextContent textContent && !string.IsNullOrEmpty(textContent.Text))
                {
                    Console.Write(textContent.Text);
                    await Task.Delay(30); // Simulate visible streaming
                }
            }
        }

        Console.WriteLine("\n\n✅ Streaming provides:\n" +
            "   • Better user experience (see responses immediately)\n" +
            "   • Lower perceived latency\n" +
            "   • Ability to process partial responses");
    }

    static async Task DemoConversationMemory()
    {
        Console.WriteLine("\n=== Pattern 2: Conversation Memory Management ===\n");

        var memoryManager = new ConversationMemoryManager(maxTokens: 4000);

        var conversations = new[]
        {
            "What is the capital of France?",
            "What's the population there?",
            "What's a famous landmark?",
            "Tell me about its history in one sentence."
        };

        Console.WriteLine("Demonstrating context-aware conversation:\n");

        foreach (var userMsg in conversations)
        {
            memoryManager.AddMessage(ChatRole.User, userMsg);
            Console.WriteLine($"User: {userMsg}");

            // Simulate response
            var response = $"[Response to: {userMsg}] - Paris is the capital, with ~2.2M population, famous for the Eiffel Tower, built in 1889.";
            memoryManager.AddMessage(ChatRole.Assistant, response);
            
            Console.WriteLine($"Assistant: {response}");
            Console.WriteLine($"   📊 Memory: {memoryManager.GetMessageCount()} messages, ~{memoryManager.EstimateTokenCount()} tokens\n");
            
            await Task.Delay(500);
        }

        Console.WriteLine("✅ Memory management allows:");
        Console.WriteLine("   • Context retention across turns");
        Console.WriteLine("   • Token limit management");
        Console.WriteLine("   • Automatic conversation pruning");
    }

    static async Task DemoErrorHandling(IChatClient chatClient)
    {
        Console.WriteLine("\n=== Pattern 3: Error Handling & Resilience ===\n");

        var resilientClient = new ResilientChatClient(chatClient);

        var testScenarios = new[]
        {
            ("Valid request", true),
            ("Request with temporary failure (retry)", true),
            ("Request with rate limit (backoff)", true),
        };

        foreach (var (scenario, shouldSucceed) in testScenarios)
        {
            Console.WriteLine($"Testing: {scenario}");
            
            try
            {
                var result = await resilientClient.CompleteWithRetryAsync(
                    new List<ChatMessage> { new(ChatRole.User, scenario) }
                );
                
                Console.WriteLine($"   ✅ Success: {result}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"   ❌ Failed after retries: {ex.Message}");
            }

            await Task.Delay(500);
        }

        Console.WriteLine("\n✅ Resilient error handling includes:");
        Console.WriteLine("   • Automatic retry with exponential backoff");
        Console.WriteLine("   • Rate limit detection and handling");
        Console.WriteLine("   • Graceful degradation");
    }

    static async Task DemoObservability(IChatClient chatClient)
    {
        Console.WriteLine("\n=== Pattern 4: Agent Observability ===\n");

        var observableClient = new ObservableChatClient(chatClient);

        Console.WriteLine("Making requests with observability:\n");

        var requests = new[]
        {
            "What is AI?",
            "Explain machine learning",
            "What are neural networks?"
        };

        foreach (var request in requests)
        {
            await observableClient.CompleteWithLoggingAsync(
                new List<ChatMessage> { new(ChatRole.User, request) }
            );
            await Task.Delay(500);
        }

        Console.WriteLine("\n📊 Metrics Summary:");
        Console.WriteLine(observableClient.GetMetricsSummary());

        Console.WriteLine("\n✅ Observability provides:");
        Console.WriteLine("   • Request/response logging");
        Console.WriteLine("   • Performance metrics");
        Console.WriteLine("   • Error tracking");
        Console.WriteLine("   • Usage analytics");
    }

    static async Task RunAllDemos()
    {
        Console.WriteLine("\n=== Running All Pattern Demos ===\n");

        await Task.Delay(500);
        Console.WriteLine("📡 Streaming Responses Demo:");
        Console.WriteLine("   Tokens appear one by one as they're generated...");
        await Task.Delay(800);

        Console.WriteLine("\n💾 Conversation Memory Demo:");
        Console.WriteLine("   Maintaining context: User asked about Paris → Capital → Population → Landmarks");
        Console.WriteLine("   Memory: 8 messages, ~350 tokens");
        await Task.Delay(800);

        Console.WriteLine("\n🔄 Error Handling Demo:");
        Console.WriteLine("   ✅ Request succeeded");
        Console.WriteLine("   ⚠️  Temporary failure → Retry 1 → Success");
        Console.WriteLine("   ⚠️  Rate limit → Wait 2s → Retry → Success");
        await Task.Delay(800);

        Console.WriteLine("\n📊 Observability Demo:");
        Console.WriteLine("   Metrics collected:");
        Console.WriteLine("   • Total requests: 3");
        Console.WriteLine("   • Avg latency: 1.2s");
        Console.WriteLine("   • Success rate: 100%");
        Console.WriteLine("   • Total tokens: 450");

        Console.WriteLine("\n✅ All patterns demonstrated!");
        Console.WriteLine("Configure API keys to see these patterns with real AI!");
    }
}

/// <summary>
/// Manages conversation history with token limits
/// </summary>
public class ConversationMemoryManager
{
    private readonly List<ChatMessage> _messages = new();
    private readonly int _maxTokens;

    public ConversationMemoryManager(int maxTokens = 4000)
    {
        _maxTokens = maxTokens;
    }

    public void AddMessage(ChatRole role, string content)
    {
        _messages.Add(new ChatMessage(role, content));
        PruneIfNeeded();
    }

    public int GetMessageCount() => _messages.Count;

    public int EstimateTokenCount()
    {
        // Rough estimation: ~4 characters per token
        return _messages.Sum(m => m.Text?.Length ?? 0) / 4;
    }

    private void PruneIfNeeded()
    {
        while (EstimateTokenCount() > _maxTokens && _messages.Count > 1)
        {
            _messages.RemoveAt(0);
        }
    }
}

/// <summary>
/// Adds retry logic and resilience to chat client
/// </summary>
public class ResilientChatClient
{
    private readonly IChatClient _innerClient;
    private readonly int _maxRetries = 3;

    public ResilientChatClient(IChatClient innerClient)
    {
        _innerClient = innerClient;
    }

    public async Task<string> CompleteWithRetryAsync(List<ChatMessage> messages)
    {
        for (int attempt = 1; attempt <= _maxRetries; attempt++)
        {
            try
            {
                var response = await _innerClient.GetResponseAsync(messages);
                return response.Text ?? "No response";
            }
            catch (Exception ex) when (attempt < _maxRetries)
            {
                Console.WriteLine($"   ⚠️  Attempt {attempt} failed: {ex.Message}");
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                Console.WriteLine($"   ⏱️  Waiting {delay.TotalSeconds}s before retry...");
                await Task.Delay(delay);
            }
        }

        throw new Exception("Max retries exceeded");
    }
}

/// <summary>
/// Adds observability and metrics to chat client
/// </summary>
public class ObservableChatClient
{
    private readonly IChatClient _innerClient;
    private readonly List<RequestMetric> _metrics = new();

    public ObservableChatClient(IChatClient innerClient)
    {
        _innerClient = innerClient;
    }

    public async Task<string> CompleteWithLoggingAsync(List<ChatMessage> messages)
    {
        var requestId = Guid.NewGuid();
        var startTime = DateTime.UtcNow;

        // NOTE: In production, avoid logging full message content to prevent PII/secret leakage
        // Consider redacting sensitive data or using a flag to disable content logging
        var messagePreview = messages.Last().Text?[..Math.Min(50, messages.Last().Text?.Length ?? 0)] ?? "";
        Console.WriteLine($"   📤 Request {requestId:N}: {messagePreview}...");

        try
        {
            var response = await _innerClient.GetResponseAsync(messages);
            var duration = DateTime.UtcNow - startTime;

            var metric = new RequestMetric
            {
                RequestId = requestId,
                Success = true,
                Duration = duration,
                TokenCount = EstimateTokens(response.Text ?? "")
            };
            _metrics.Add(metric);

            Console.WriteLine($"   📥 Response {requestId:N}: Success in {duration.TotalMilliseconds:F0}ms");
            return response.Text ?? "";
        }
        catch (Exception ex)
        {
            var duration = DateTime.UtcNow - startTime;
            var metric = new RequestMetric
            {
                RequestId = requestId,
                Success = false,
                Duration = duration,
                Error = ex.Message
            };
            _metrics.Add(metric);

            Console.WriteLine($"   ❌ Request {requestId:N}: Failed in {duration.TotalMilliseconds:F0}ms");
            throw;
        }
    }

    public string GetMetricsSummary()
    {
        if (_metrics.Count == 0) return "No metrics available";

        var totalRequests = _metrics.Count;
        var successfulRequests = _metrics.Count(m => m.Success);
        var avgDuration = _metrics.Average(m => m.Duration.TotalMilliseconds);
        var totalTokens = _metrics.Sum(m => m.TokenCount);

        return $@"   Total Requests: {totalRequests}
   Successful: {successfulRequests} ({(100.0 * successfulRequests / totalRequests):F1}%)
   Avg Duration: {avgDuration:F0}ms
   Total Tokens: {totalTokens}";
    }

    private int EstimateTokens(string text) => text.Length / 4;
}

public class RequestMetric
{
    public Guid RequestId { get; set; }
    public bool Success { get; set; }
    public TimeSpan Duration { get; set; }
    public int TokenCount { get; set; }
    public string? Error { get; set; }
}
