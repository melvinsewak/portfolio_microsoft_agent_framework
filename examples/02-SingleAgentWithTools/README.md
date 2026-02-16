# Example 2: Single Agent with Tools

## Overview
This example demonstrates an AI agent with tool/function calling capabilities. The agent can:
- Invoke C# methods as tools
- Perform calculations
- Retrieve weather information
- Manage tasks
- Automatically select appropriate tools based on user requests

## Key Concepts

### Tool Pattern
The Tool Pattern (also known as Function Calling) allows AI agents to execute code and access external systems. Instead of just generating text, the agent can take actions.

### Function Definitions
Tools are defined using C# methods decorated with `[Description]` attributes:

```csharp
[Description("Performs basic arithmetic operations")]
public string Calculate(
    [Description("Operation: add, subtract, multiply, or divide")] string operation,
    [Description("First number")] double number1,
    [Description("Second number")] double number2)
{
    // Implementation
}
```

### Tool Registration
Tools are registered with the chat client using `AIFunctionFactory`:

```csharp
var options = new ChatOptions
{
    Tools = [
        AIFunctionFactory.Create(tools.Calculate),
        AIFunctionFactory.Create(tools.GetWeather),
        AIFunctionFactory.Create(tools.AddTask),
        AIFunctionFactory.Create(tools.ListTasks)
    ]
};
```

### How It Works

1. **User Request**: "What's 25 * 48?"
2. **Agent Analysis**: AI determines it needs to use the Calculator tool
3. **Tool Invocation**: The `Calculate` method is called with parameters
4. **Result Integration**: Tool result is added to conversation context
5. **Final Response**: AI generates natural language response with the result

## Available Tools

### 📊 Calculator
Performs arithmetic operations: add, subtract, multiply, divide
```csharp
Calculate("multiply", 25, 48) // Returns: "Result: 1200"
```

### 🌤️ Weather
Gets weather information for a location (simulated in this example)
```csharp
GetWeather("Seattle") // Returns: "Weather in Seattle: Sunny, 83°F"
```

### ✅ Task Management
- `AddTask(description)`: Adds a new task to the list
- `ListTasks()`: Retrieves all current tasks

## Running the Example

### Prerequisites
1. .NET 10.0 SDK or later
2. Azure OpenAI service with API key (for real tool calling)

### Configuration
Same as Example 1 - use environment variables or `appsettings.json`:

```bash
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"
export AZURE_OPENAI_API_KEY="your-api-key"
export AZURE_OPENAI_DEPLOYMENT="gpt-4"
```

### Run
```bash
dotnet run
```

### Example Interactions

```
You: What's 25 * 48?
   🔧 Tool called: Calculate(multiply, 25, 48)
Assistant: The result of 25 multiplied by 48 is 1200.

You: What's the weather in Seattle?
   🔧 Tool called: GetWeather(Seattle)
Assistant: The weather in Seattle is currently Sunny with a temperature of 83°F.

You: Add task: Review code
   🔧 Tool called: AddTask('Review code')
Assistant: I've added "Review code" to your task list.
```

## Code Structure

```
Program.cs
├── Main()
│   ├── Configuration Setup
│   ├── Create Chat Client
│   ├── Initialize Tools
│   ├── Register Tools with ChatOptions
│   └── Interactive Loop
│       ├── User Input
│       ├── Tool Selection (automatic)
│       ├── Tool Execution
│       └── Response Generation
│
└── AgentTools Class
    ├── Calculate()
    ├── GetWeather()
    ├── AddTask()
    └── ListTasks()
```

## Tool Implementation Best Practices

### 1. Clear Descriptions
Use detailed descriptions for both the function and its parameters:
```csharp
[Description("Gets the current weather for a specified location")]
public string GetWeather(
    [Description("City name")] string location)
```

### 2. Type Safety
Use appropriate C# types for parameters:
```csharp
public string Calculate(string operation, double number1, double number2)
```

### 3. Error Handling
Handle edge cases gracefully:
```csharp
"divide" when number2 != 0 => $"Result: {number1 / number2}",
"divide" => "Error: Cannot divide by zero",
```

### 4. Logging
Log tool invocations for debugging and monitoring:
```csharp
Console.WriteLine($"   🔧 Tool called: Calculate({operation}, {number1}, {number2})");
```

## Advanced Tool Patterns

### Async Tools
For I/O operations, use async methods:
```csharp
[Description("Fetches data from an API")]
public async Task<string> FetchDataAsync(string url)
{
    // IMPORTANT: Validate URLs to prevent SSRF attacks
    if (!Uri.TryCreate(url, UriKind.Absolute, out var uri) ||
        (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
    {
        throw new ArgumentException("Invalid URL. Only absolute HTTP/HTTPS URLs are allowed.", nameof(url));
    }

    // Allowlist of external APIs this tool is permitted to call
    var allowedHosts = new[] { "api.example.com", "contoso.com" };
    if (!Array.Exists(allowedHosts, h => string.Equals(h, uri.Host, StringComparison.OrdinalIgnoreCase)))
    {
        throw new InvalidOperationException("The requested URL host is not allowed.");
    }

    using var client = new HttpClient();
    return await client.GetStringAsync(uri);
}
```

### Tool Chaining
Tools can call other tools:
```csharp
[Description("Books travel and sends confirmation")]
public string BookAndNotify(string destination)
{
    var bookingResult = BookFlight(destination);
    SendEmail($"Flight booked: {bookingResult}");
    return bookingResult;
}
```

### Context-Aware Tools
Access conversation history or application state:
```csharp
public string GetUserPreference(string category)
{
    // Access user profile or settings
    return userPreferences[category];
}
```

## Security Considerations

1. **Input Validation**: Always validate tool inputs
2. **Rate Limiting**: Implement rate limits for expensive operations
3. **Access Control**: Verify permissions before executing sensitive operations
4. **Audit Logging**: Log all tool invocations for security audits

## Next Steps

- Explore [Multi-Agent Orchestrator](../03-MultiAgentOrchestrator/) to see specialized agents
- Learn about [Advanced Patterns](../04-AdvancedPatterns/) for production scenarios
- Implement your own custom tools for your domain

## Related Resources

- [Function Calling in Azure OpenAI](https://learn.microsoft.com/en-us/azure/ai-services/openai/how-to/function-calling)
- [Microsoft.Extensions.AI AIFunctionFactory](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.ai.aifunctionfactory)
- [Tool Use Best Practices](https://microsoft.github.io/ai-agents-for-beginners/04-tool-use/)
