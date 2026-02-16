# Example 3: Multi-Agent Orchestrator

## Overview
This example demonstrates the **Orchestrator Pattern** for coordinating multiple specialized AI agents. Key features:
- Multiple agents with distinct responsibilities
- Central orchestrator for task delegation
- Specialized domain expertise per agent
- Coordinated multi-agent responses

## Architecture

### The Orchestrator Pattern
```
┌──────────────────────────────────────┐
│         User Request                 │
└──────────────┬───────────────────────┘
               ▼
┌──────────────────────────────────────┐
│       Orchestrator Agent             │
│  (Analyzes and delegates tasks)      │
└─────┬────────┬────────┬──────────────┘
      ▼        ▼        ▼
┌──────────┐ ┌──────────┐ ┌──────────┐
│ Travel   │ │ Calendar │ │  Email   │
│  Agent   │ │  Agent   │ │  Agent   │
└──────────┘ └──────────┘ └──────────┘
      │          │          │
      └──────────┴──────────┘
               ▼
    ┌──────────────────────┐
    │  Aggregated Response │
    └──────────────────────┘
```

## Specialized Agents

### ✈️ Travel Agent
**Responsibilities:**
- Flight bookings
- Hotel reservations
- Car rentals
- Travel itinerary management

**Example:**
```csharp
public class TravelAgent
{
    public async Task<string> HandleRequestAsync(string request)
    {
        // Process travel-related requests
        return "Flight booked: AA123, Departure 10:00 AM";
    }
}
```

### 📅 Calendar Agent
**Responsibilities:**
- Schedule meetings
- Manage appointments
- Check availability
- Send meeting invitations

**Example:**
```csharp
public class CalendarAgent
{
    public async Task<string> HandleRequestAsync(string request)
    {
        // Process calendar requests
        return "Meeting scheduled: Wednesday, 2:00 PM";
    }
}
```

### 📧 Email Agent
**Responsibilities:**
- Send emails
- Manage inbox
- Draft messages
- Email confirmations

**Example:**
```csharp
public class EmailAgent
{
    public async Task<string> HandleRequestAsync(string request)
    {
        // Process email requests
        return "Email sent to recipients";
    }
}
```

## Orchestrator Implementation

### Task Analysis
The orchestrator analyzes user requests to determine which agents to involve:

```csharp
public async Task<string> ProcessRequestAsync(string userRequest)
{
    var results = new List<string>();

    // Delegate to appropriate agents
    if (userRequest.Contains("flight") || userRequest.Contains("hotel"))
    {
        var result = await _travelAgent.HandleRequestAsync(userRequest);
        results.Add($"✈️  Travel: {result}");
    }

    if (userRequest.Contains("meeting") || userRequest.Contains("schedule"))
    {
        var result = await _calendarAgent.HandleRequestAsync(userRequest);
        results.Add($"📅 Calendar: {result}");
    }

    // Aggregate and return results
    return string.Join("\n", results);
}
```

## Running the Example

### Prerequisites
1. .NET 10.0 SDK or later
2. Azure OpenAI service with API key

### Configuration
```bash
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"
export AZURE_OPENAI_API_KEY="your-api-key"
export AZURE_OPENAI_DEPLOYMENT="gpt-4"
```

### Run
```bash
dotnet run
```

## Example Scenarios

### Single Agent Invocation
```
You: Book a flight to NYC next Tuesday

🎯 Orchestrator analyzing request...
   ✈️  Travel Agent processing...

Orchestrator:
✈️  Travel: Flight booked successfully (Flight AA123, Dep: 10:00 AM)
```

### Multi-Agent Coordination
```
You: Book a hotel in SF for Monday and send confirmation email

🎯 Orchestrator coordinating multiple agents:
   ✈️  Travel Agent: Hotel booked - Grand Hotel SF
   📧 Email Agent: Confirmation email sent

Orchestrator:
✈️  Travel: Hotel reserved (Grand Hotel, Check-in: Mon 2/23)
📧 Email: Email sent successfully to recipients
```

## Design Patterns

### 1. Delegation Pattern
The orchestrator delegates specific tasks to specialized agents without handling the details itself.

**Benefits:**
- Clear separation of concerns
- Easier maintenance and testing
- Independent scaling of agents

### 2. Aggregation Pattern
Results from multiple agents are collected and synthesized into a cohesive response.

```csharp
var travelResult = await _travelAgent.HandleRequestAsync(request);
var emailResult = await _emailAgent.HandleRequestAsync(request);

return $"Travel: {travelResult}\nEmail: {emailResult}";
```

### 3. Workflow Pattern
Complex multi-step processes can be orchestrated:

```csharp
// Step 1: Book travel
var booking = await _travelAgent.BookFlightAsync(details);

// Step 2: Add to calendar
await _calendarAgent.AddEventAsync(booking.Date, booking.Details);

// Step 3: Send confirmation
await _emailAgent.SendConfirmationAsync(booking);
```

## Advanced Orchestration

### Parallel Execution (Optional Pattern)
For independent tasks, you can execute agent calls concurrently:

```csharp
var tasks = new[]
{
    _travelAgent.HandleRequestAsync(userRequest),
    _calendarAgent.HandleRequestAsync(userRequest),
    _emailAgent.HandleRequestAsync(userRequest)
};

var results = await Task.WhenAll(tasks);
```

**Note**: The current example executes agents sequentially for simplicity. Use parallel execution when agents don't depend on each other's results.

### Conditional Workflows
Dynamic routing based on context:

```csharp
if (userContext.IsUrgent)
{
    // Priority handling
    await _emailAgent.SendUrgentNotificationAsync();
}
```

### Error Handling
Graceful degradation when agents fail:

```csharp
try
{
    var result = await _travelAgent.HandleRequestAsync(request);
    return result;
}
catch (Exception ex)
{
    return "Travel booking is temporarily unavailable. Please try again.";
}
```

## Real-World Applications

### Enterprise Assistant
- Email management
- Calendar scheduling
- Document processing
- Task tracking

### Customer Service
- Order management agent
- Support ticket agent
- Refund processing agent
- Knowledge base agent

### Smart Home
- Lighting control agent
- Temperature management agent
- Security monitoring agent
- Entertainment system agent

## Benefits of Multi-Agent Systems

1. **Modularity**: Each agent can be developed, tested, and deployed independently
2. **Scalability**: Scale individual agents based on demand
3. **Maintainability**: Changes to one agent don't affect others
4. **Specialization**: Each agent can use optimal tools and models for its domain
5. **Fault Isolation**: Failure of one agent doesn't bring down the entire system

## Best Practices

### Agent Design
- **Single Responsibility**: Each agent should have one clear purpose
- **Loose Coupling**: Minimize dependencies between agents
- **Clear Contracts**: Define clear input/output interfaces

### Orchestrator Design
- **Smart Routing**: Use AI to determine which agents to invoke
- **Context Preservation**: Maintain conversation context across agent invocations
- **Result Synthesis**: Combine agent responses into coherent outputs

### Production Considerations
- **Monitoring**: Track agent performance and errors
- **Logging**: Log all orchestrator decisions and agent invocations
- **Timeout Handling**: Set reasonable timeouts for agent operations
- **Circuit Breakers**: Prevent cascading failures

## Next Steps

- Explore [Advanced Patterns](../04-AdvancedPatterns/) for production features
- Implement custom specialized agents for your domain
- Add sophisticated routing logic with AI-based decision making

## Related Resources

- [Multi-Agent Systems Overview](https://microsoft.github.io/ai-agents-for-beginners/)
- [Agent Orchestration Patterns](https://learn.microsoft.com/en-us/agent-framework/)
- [Workflow Orchestration](https://learn.microsoft.com/en-us/azure/logic-apps/)
