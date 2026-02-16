# Example 1: Basic Chatbot

## Overview
This example demonstrates a basic conversational AI chatbot using the Microsoft Agent Framework. It showcases:
- Simple single-agent pattern
- Conversation history management
- Context retention across multiple turns
- Configuration management

## Key Concepts

### Single Agent Pattern
The single agent pattern is the foundation of all agent-based applications. In this pattern, one AI agent handles all user interactions, maintaining context and providing responses based on conversation history.

### Conversation History
The chatbot maintains a list of messages representing the entire conversation:
- **System Message**: Defines the agent's behavior and personality
- **User Messages**: Input from the user
- **Assistant Messages**: Responses from the AI

```csharp
var conversationHistory = new List<ChatMessage>
{
    new ChatMessage(ChatRole.System, "You are a friendly and helpful assistant.")
};
```

### Microsoft.Extensions.AI Integration
This example uses the `Microsoft.Extensions.AI` abstractions to interact with Azure OpenAI:

```csharp
var azureClient = new AzureOpenAIClient(new Uri(endpoint), new AzureKeyCredential(apiKey));
var chatClient = azureClient.GetChatClient(deploymentName).AsIChatClient();
```

## Running the Example

### Prerequisites
1. .NET 10.0 SDK or later
2. Azure OpenAI service with API key

### Configuration Options

**Option 1: Environment Variables**
```bash
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"
export AZURE_OPENAI_API_KEY="your-api-key"
export AZURE_OPENAI_DEPLOYMENT="gpt-4"
```

**Option 2: Configuration File**
```bash
cp appsettings.template.json appsettings.json
# Edit appsettings.json with your credentials
```

### Run
```bash
dotnet run
```

### Demo Mode
If no API keys are configured, the example runs in demo mode to showcase the conversation flow.

## Code Structure

```
Program.cs
├── Main()
│   ├── Load Configuration
│   ├── Create Azure OpenAI Client
│   ├── Initialize Conversation History
│   └── Conversation Loop
│       ├── Get User Input
│       ├── Add to History
│       ├── Get AI Response
│       └── Display Response
└── RunDemoMode()
    └── Simulated Conversation
```

## Key Takeaways

1. **Context Management**: Each turn in the conversation includes all previous messages, allowing the AI to maintain context.

2. **Stateless API**: The AI service is stateless; the client must maintain and send the full conversation history with each request.

3. **Graceful Degradation**: The demo mode allows testing and understanding the flow without requiring API credentials.

## Next Steps

- Explore [Example 2: Single Agent with Tools](../02-SingleAgentWithTools/) to add function calling capabilities
- Learn about [Multi-Agent Orchestration](../03-MultiAgentOrchestrator/) for complex workflows
- Study [Advanced Patterns](../04-AdvancedPatterns/) for production-ready features

## Related Resources

- [Microsoft Agent Framework Documentation](https://learn.microsoft.com/en-us/agent-framework/)
- [IChatClient Interface](https://learn.microsoft.com/en-us/dotnet/ai/ichatclient)
- [Azure OpenAI Service](https://learn.microsoft.com/en-us/azure/ai-services/openai/)
