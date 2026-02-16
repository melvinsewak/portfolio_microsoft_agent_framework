using Microsoft.Extensions.AI;
using Azure.AI.OpenAI;
using Azure;
using Microsoft.Extensions.Configuration;

namespace BasicChatbot;

/// <summary>
/// Example 1: Basic Chatbot using Microsoft.Extensions.AI
/// This demonstrates a simple single-agent chatbot with conversation history.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Example 1: Basic Chatbot ===\n");
        Console.WriteLine("This example demonstrates a basic chatbot using Microsoft.Extensions.AI.");
        Console.WriteLine("The chatbot maintains conversation history and context.\n");

        // Load configuration
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var endpoint = configuration["AzureOpenAI:Endpoint"] ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        var apiKey = configuration["AzureOpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        var deploymentName = configuration["AzureOpenAI:DeploymentName"] ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? "gpt-4";

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("⚠️  Configuration required:");
            Console.WriteLine("Please set the following environment variables or add them to appsettings.json:");
            Console.WriteLine("  - AZURE_OPENAI_ENDPOINT (e.g., https://your-resource.openai.azure.com/)");
            Console.WriteLine("  - AZURE_OPENAI_API_KEY");
            Console.WriteLine("  - AZURE_OPENAI_DEPLOYMENT (optional, defaults to 'gpt-4')");
            Console.WriteLine("\nThis is a DEMO that shows the structure and patterns.");
            Console.WriteLine("In production, you would configure these values properly.\n");
            
            // Run in demo mode
            await RunDemoMode();
            return;
        }

        try
        {
            // Create the Azure OpenAI client
            var azureClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
            
            // Get the chat client using Microsoft.Extensions.AI
            var chatClient = azureClient.GetChatClient(deploymentName).AsIChatClient();

            // Conversation history
            var conversationHistory = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, "You are a friendly and helpful assistant. Provide concise, accurate answers.")
            };

            Console.WriteLine("💬 Chat with the AI assistant (type 'exit' or 'quit' to end)\n");

            while (true)
            {
                Console.Write("You: ");
                var userInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(userInput) || 
                    userInput.Equals("exit", StringComparison.OrdinalIgnoreCase) || 
                    userInput.Equals("quit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("\nGoodbye!");
                    break;
                }

                // Add user message to history
                conversationHistory.Add(new ChatMessage(ChatRole.User, userInput));

                // Get response from the chatbot
                var response = await chatClient.GetResponseAsync(conversationHistory);

                // Add assistant response to history
                conversationHistory.AddRange(response.Messages);

                Console.WriteLine($"Assistant: {response.Text}\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
            Console.WriteLine("\nPlease check your configuration and try again.");
        }
    }

    static async Task RunDemoMode()
    {
        Console.WriteLine("=== Running in DEMO Mode ===\n");
        Console.WriteLine("This demonstrates the conversation flow without actual API calls:\n");

        var demoConversation = new[]
        {
            ("You: What can you help me with?", "Assistant: I can help you with a wide range of tasks including answering questions, providing information, writing assistance, problem-solving, and much more. What would you like help with today?"),
            ("You: Tell me about Microsoft Agents Framework", "Assistant: Microsoft Agents Framework is a powerful platform for building AI agents in C# and .NET. It provides tools for creating single agents, multi-agent systems, and supports features like conversation memory, tool/function calling, and orchestration patterns."),
            ("You: What are the key features?", "Assistant: Key features include:\n- Single and multi-agent support\n- Conversation history and context management\n- Tool/function calling capabilities\n- Multiple design patterns (orchestrator, workflow, proxy)\n- Integration with Azure OpenAI and other AI services\n- Built on Microsoft.Extensions.AI abstractions")
        };

        foreach (var (user, assistant) in demoConversation)
        {
            Console.WriteLine(user);
            await Task.Delay(500);
            Console.WriteLine($"{assistant}\n");
            await Task.Delay(1000);
        }

        Console.WriteLine("Demo complete! Configure your API keys to run with real AI.");
    }
}
