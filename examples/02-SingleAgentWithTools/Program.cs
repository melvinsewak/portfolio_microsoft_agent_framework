using Microsoft.Extensions.AI;
using Azure.AI.OpenAI;
using Azure;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;

namespace SingleAgentWithTools;

/// <summary>
/// Example 2: Single Agent with Tools (Function Calling)
/// This demonstrates an AI agent that can use tools/functions to perform actions.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Example 2: Single Agent with Tools ===\n");
        Console.WriteLine("This example demonstrates an AI agent with tool/function calling capabilities.");
        Console.WriteLine("The agent can use tools like calculator, weather lookup, and task management.\n");

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
            Console.WriteLine("  - AZURE_OPENAI_ENDPOINT");
            Console.WriteLine("  - AZURE_OPENAI_API_KEY");
            Console.WriteLine("  - AZURE_OPENAI_DEPLOYMENT (optional)\n");
            
            await RunDemoMode();
            return;
        }

        try
        {
            // Create the Azure OpenAI client with tools
            var azureClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
            var chatClient = azureClient.GetChatClient(deploymentName).AsIChatClient();

            // Create tools for the agent
            var tools = new AgentTools();
            
            // Configure chat client with tools
            var options = new ChatOptions
            {
                Tools = [
                    AIFunctionFactory.Create(tools.Calculate),
                    AIFunctionFactory.Create(tools.GetWeather),
                    AIFunctionFactory.Create(tools.AddTask),
                    AIFunctionFactory.Create(tools.ListTasks)
                ]
            };

            Console.WriteLine("Available tools:");
            Console.WriteLine("  📊 Calculator - Perform arithmetic operations");
            Console.WriteLine("  🌤️  Weather - Get weather information");
            Console.WriteLine("  ✅ Tasks - Manage a to-do list\n");
            Console.WriteLine("Try asking: 'What's 25 * 48?', 'What's the weather in Seattle?', or 'Add task: Review code'\n");

            var conversationHistory = new List<ChatMessage>
            {
                new ChatMessage(ChatRole.System, 
                    "You are a helpful assistant with access to tools. " +
                    "Use the tools when appropriate to help the user.")
            };

            Console.WriteLine("💬 Chat with the tool-enabled assistant (type 'exit' to end)\n");

            while (true)
            {
                Console.Write("You: ");
                var userInput = Console.ReadLine();

                if (string.IsNullOrWhiteSpace(userInput) || 
                    userInput.Equals("exit", StringComparison.OrdinalIgnoreCase))
                {
                    Console.WriteLine("\nGoodbye!");
                    break;
                }

                conversationHistory.Add(new ChatMessage(ChatRole.User, userInput));

                // Get response with tool calling
                var response = await chatClient.GetResponseAsync(conversationHistory, options);

                conversationHistory.AddRange(response.Messages);
                Console.WriteLine($"Assistant: {response.Text}\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
        }
    }

    static async Task RunDemoMode()
    {
        Console.WriteLine("=== Running in DEMO Mode ===\n");
        
        var tools = new AgentTools();
        
        Console.WriteLine("Demonstrating tool capabilities:\n");
        
        Console.WriteLine("🔢 Calculator Tool:");
        var calcResult = tools.Calculate("multiply", 25, 48);
        Console.WriteLine($"   Calculate 25 * 48 = {calcResult}\n");
        await Task.Delay(500);

        Console.WriteLine("🌤️  Weather Tool:");
        var weather = tools.GetWeather("Seattle");
        Console.WriteLine($"   {weather}\n");
        await Task.Delay(500);

        Console.WriteLine("✅ Task Management:");
        tools.AddTask("Review pull request");
        tools.AddTask("Update documentation");
        Console.WriteLine($"   Tasks added!");
        var tasks = tools.ListTasks();
        Console.WriteLine($"   {tasks}\n");
        await Task.Delay(500);

        Console.WriteLine("This demonstrates how agents can use tools to perform actions.");
        Console.WriteLine("Configure your API keys to interact with the agent in real-time!");
    }
}

/// <summary>
/// Collection of tools/functions that the AI agent can use
/// </summary>
public class AgentTools
{
    private readonly List<string> tasks = new();

    [Description("Performs basic arithmetic operations (add, subtract, multiply, divide)")]
    public string Calculate(
        [Description("Operation: add, subtract, multiply, or divide")] string operation,
        [Description("First number")] double number1,
        [Description("Second number")] double number2)
    {
        Console.WriteLine($"   🔧 Tool called: Calculate({operation}, {number1}, {number2})");
        
        return operation.ToLower() switch
        {
            "add" => $"Result: {number1 + number2}",
            "subtract" => $"Result: {number1 - number2}",
            "multiply" => $"Result: {number1 * number2}",
            "divide" when number2 != 0 => $"Result: {number1 / number2}",
            "divide" => "Error: Cannot divide by zero",
            _ => $"Error: Unknown operation '{operation}'"
        };
    }

    [Description("Gets the current weather for a specified location")]
    public string GetWeather(
        [Description("City name")] string location)
    {
        Console.WriteLine($"   🔧 Tool called: GetWeather({location})");
        
        // Simulated weather data (in production, would call a real API)
        var weatherConditions = new[] { "Sunny", "Cloudy", "Rainy", "Partly cloudy" };
        var random = new Random(location.GetHashCode());
        var condition = weatherConditions[random.Next(weatherConditions.Length)];
        var temperature = random.Next(50, 85);
        
        return $"Weather in {location}: {condition}, {temperature}°F";
    }

    [Description("Adds a new task to the to-do list")]
    public string AddTask(
        [Description("Task description")] string taskDescription)
    {
        Console.WriteLine($"   🔧 Tool called: AddTask('{taskDescription}')");
        
        tasks.Add(taskDescription);
        return $"Task added: '{taskDescription}' (Total tasks: {tasks.Count})";
    }

    [Description("Lists all current tasks in the to-do list")]
    public string ListTasks()
    {
        Console.WriteLine($"   🔧 Tool called: ListTasks()");
        
        if (tasks.Count == 0)
        {
            return "No tasks in the list.";
        }

        var taskList = string.Join("\n", tasks.Select((t, i) => $"{i + 1}. {t}"));
        return $"Current tasks:\n{taskList}";
    }
}
