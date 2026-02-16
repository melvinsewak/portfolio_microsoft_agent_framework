using Microsoft.Extensions.AI;
using Azure.AI.OpenAI;
using Azure;
using Microsoft.Extensions.Configuration;
using System.ComponentModel;

namespace MultiAgentOrchestrator;

/// <summary>
/// Example 3: Multi-Agent Orchestrator Pattern
/// This demonstrates multiple specialized agents working together under an orchestrator.
/// </summary>
class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Example 3: Multi-Agent Orchestrator ===\n");
        Console.WriteLine("This example demonstrates multiple specialized agents coordinated by an orchestrator.");
        Console.WriteLine("Agents: Travel Agent, Calendar Agent, Email Agent\n");

        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var endpoint = configuration["AzureOpenAI:Endpoint"] ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT");
        var apiKey = configuration["AzureOpenAI:ApiKey"] ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_API_KEY");
        var deploymentName = configuration["AzureOpenAI:DeploymentName"] ?? Environment.GetEnvironmentVariable("AZURE_OPENAI_DEPLOYMENT") ?? "gpt-4";

        if (string.IsNullOrEmpty(endpoint) || string.IsNullOrEmpty(apiKey))
        {
            Console.WriteLine("⚠️  Configuration not found. Running in DEMO mode.\n");
            await RunDemoMode();
            return;
        }

        try
        {
            var azureClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
            
            // Create specialized agents
            var travelAgent = new TravelAgent(azureClient.GetChatClient(deploymentName).AsIChatClient());
            var calendarAgent = new CalendarAgent(azureClient.GetChatClient(deploymentName).AsIChatClient());
            var emailAgent = new EmailAgent(azureClient.GetChatClient(deploymentName).AsIChatClient());
            
            // Create orchestrator
            var orchestrator = new OrchestratorAgent(
                azureClient.GetChatClient(deploymentName).AsIChatClient(),
                travelAgent,
                calendarAgent,
                emailAgent
            );

            Console.WriteLine("🎯 Available agents:");
            Console.WriteLine("  ✈️  Travel Agent - Book flights, hotels, rentals");
            Console.WriteLine("  📅 Calendar Agent - Manage schedule and meetings");
            Console.WriteLine("  📧 Email Agent - Send and manage emails\n");
            Console.WriteLine("Try: 'Book a flight to NYC next Tuesday and schedule a meeting'\n");

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

                var result = await orchestrator.ProcessRequestAsync(userInput);
                Console.WriteLine($"\nOrchestrator: {result}\n");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"\n❌ Error: {ex.Message}");
        }
    }

    static async Task RunDemoMode()
    {
        Console.WriteLine("=== Multi-Agent Orchestration Demo ===\n");
        
        var scenarios = new[]
        {
            ("Book a flight to San Francisco next Monday",
             "✈️  Travel Agent: Flight booked to San Francisco on 2026-02-23\n   Flight: AA123, Departure: 10:00 AM, Arrival: 1:00 PM"),
            
            ("Schedule a team meeting for next Wednesday at 2 PM",
             "📅 Calendar Agent: Meeting scheduled\n   Title: Team Meeting\n   Time: Wednesday, 2026-02-25 at 2:00 PM\n   Duration: 1 hour"),
            
            ("Send an email to the team about the meeting",
             "📧 Email Agent: Email sent\n   To: team@company.com\n   Subject: Team Meeting on Wednesday\n   Status: Delivered"),
            
            ("Book a hotel in SF for Monday night and send confirmation email",
             "🎯 Orchestrator coordinating multiple agents:\n" +
             "✈️  Travel Agent: Hotel booked - Grand Hotel SF, Check-in: Mon 2/23\n" +
             "📧 Email Agent: Confirmation email sent with booking details")
        };

        foreach (var (request, response) in scenarios)
        {
            Console.WriteLine($"User: {request}");
            await Task.Delay(500);
            Console.WriteLine($"\n{response}\n");
            await Task.Delay(1000);
        }

        Console.WriteLine("\nDemo complete! This shows how multiple specialized agents work together.");
        Console.WriteLine("Configure API keys to run with real AI coordination!");
    }
}

/// <summary>
/// Orchestrator that delegates tasks to specialized agents
/// </summary>
public class OrchestratorAgent
{
    private readonly IChatClient _chatClient;
    private readonly TravelAgent _travelAgent;
    private readonly CalendarAgent _calendarAgent;
    private readonly EmailAgent _emailAgent;

    public OrchestratorAgent(
        IChatClient chatClient,
        TravelAgent travelAgent,
        CalendarAgent calendarAgent,
        EmailAgent emailAgent)
    {
        _chatClient = chatClient;
        _travelAgent = travelAgent;
        _calendarAgent = calendarAgent;
        _emailAgent = emailAgent;
    }

    public async Task<string> ProcessRequestAsync(string userRequest)
    {
        Console.WriteLine("\n🎯 Orchestrator analyzing request...");

        // Determine which agents to use (simplified logic for demo)
        var results = new List<string>();

        if (userRequest.Contains("flight", StringComparison.OrdinalIgnoreCase) || 
            userRequest.Contains("hotel", StringComparison.OrdinalIgnoreCase))
        {
            var travelResult = await _travelAgent.HandleRequestAsync(userRequest);
            results.Add($"✈️  Travel: {travelResult}");
        }

        if (userRequest.Contains("meeting", StringComparison.OrdinalIgnoreCase) || 
            userRequest.Contains("schedule", StringComparison.OrdinalIgnoreCase))
        {
            var calendarResult = await _calendarAgent.HandleRequestAsync(userRequest);
            results.Add($"📅 Calendar: {calendarResult}");
        }

        if (userRequest.Contains("email", StringComparison.OrdinalIgnoreCase) || 
            userRequest.Contains("send", StringComparison.OrdinalIgnoreCase))
        {
            var emailResult = await _emailAgent.HandleRequestAsync(userRequest);
            results.Add($"📧 Email: {emailResult}");
        }

        if (results.Count == 0)
        {
            return "I'm not sure which agent to use for this request. Try asking about travel, calendar, or email tasks.";
        }

        return string.Join("\n", results);
    }
}

/// <summary>
/// Specialized agent for travel bookings
/// </summary>
public class TravelAgent
{
    private readonly IChatClient _chatClient;

    public TravelAgent(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<string> HandleRequestAsync(string request)
    {
        Console.WriteLine("   ✈️  Travel Agent processing...");
        
        // Simulate travel booking (in production, would use real APIs)
        await Task.Delay(500);
        
        if (request.Contains("flight", StringComparison.OrdinalIgnoreCase))
        {
            return "Flight booked successfully (Flight AA123, Dep: 10:00 AM)";
        }
        if (request.Contains("hotel", StringComparison.OrdinalIgnoreCase))
        {
            return "Hotel reserved (Grand Hotel, Check-in: Tomorrow)";
        }
        return "Travel arrangement completed";
    }
}

/// <summary>
/// Specialized agent for calendar management
/// </summary>
public class CalendarAgent
{
    private readonly IChatClient _chatClient;

    public CalendarAgent(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<string> HandleRequestAsync(string request)
    {
        Console.WriteLine("   📅 Calendar Agent processing...");
        
        await Task.Delay(500);
        
        if (request.Contains("meeting", StringComparison.OrdinalIgnoreCase))
        {
            return "Meeting scheduled successfully (Next Wednesday, 2:00 PM)";
        }
        return "Calendar event created";
    }
}

/// <summary>
/// Specialized agent for email management
/// </summary>
public class EmailAgent
{
    private readonly IChatClient _chatClient;

    public EmailAgent(IChatClient chatClient)
    {
        _chatClient = chatClient;
    }

    public async Task<string> HandleRequestAsync(string request)
    {
        Console.WriteLine("   📧 Email Agent processing...");
        
        await Task.Delay(500);
        
        return "Email sent successfully to recipients";
    }
}
