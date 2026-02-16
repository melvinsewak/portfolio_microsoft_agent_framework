# Microsoft Agent Framework - Production Examples Portfolio

This repository demonstrates production-ready use cases and design patterns for building AI agents using the **Microsoft Agent Framework** (previously known as Microsoft Agents Framework) with C# and .NET.

## 📚 Overview

The Microsoft Agent Framework is Microsoft's unified platform for building intelligent, action-oriented AI agents. This portfolio covers key patterns and real-world scenarios for implementing single agents, multi-agent systems, and advanced agent capabilities.

**Official Documentation**: [Microsoft Agent Framework Documentation](https://learn.microsoft.com/en-us/agent-framework/overview/?pivots=programming-language-csharp)

## 🎯 Examples Included

### 1. Basic Chatbot (Single Agent Pattern)
**Location**: `examples/01-BasicChatbot/`

A simple conversational AI agent that demonstrates:
- Single agent conversation flow
- Conversation history management
- Context retention across multiple turns
- Basic system prompts and user interactions

**Key Concepts**:
- Using `Microsoft.Extensions.AI` abstractions
- Integration with Azure OpenAI
- Managing conversation state
- Graceful error handling

### 2. Single Agent with Tools (Function Calling)
**Location**: `examples/02-SingleAgentWithTools/`

An AI agent with tool/function calling capabilities:
- Calculator tool for arithmetic operations
- Weather lookup tool
- Task management (add/list tasks)
- Dynamic tool selection based on user intent

**Key Concepts**:
- Function/tool definitions with `[Description]` attributes
- Tool registration with `AIFunctionFactory`
- Automatic tool selection by the AI
- Tool execution and result integration

### 3. Multi-Agent Orchestrator Pattern
**Location**: `examples/03-MultiAgentOrchestrator/`

Demonstrates multiple specialized agents working together:
- **Travel Agent**: Handles flight and hotel bookings
- **Calendar Agent**: Manages scheduling and meetings
- **Email Agent**: Handles email communications
- **Orchestrator**: Coordinates and delegates to specialized agents

**Key Concepts**:
- Agent specialization and separation of concerns
- Orchestrator pattern for task delegation
- Multi-agent coordination
- Combining results from multiple agents

### 4. Advanced Patterns
**Location**: `examples/04-AdvancedPatterns/`

Production-ready patterns and best practices:
- **Streaming Responses**: Token-by-token response generation
- **Conversation Memory Management**: Token limit handling and pruning
- **Error Handling & Resilience**: Retry logic with exponential backoff
- **Observability**: Request logging, metrics, and monitoring

**Key Concepts**:
- Streaming APIs for better UX
- Memory management strategies
- Production-grade error handling
- Performance monitoring and metrics

## 🚀 Getting Started

### Prerequisites

- **.NET 8.0 SDK** or later ([Download](https://dotnet.microsoft.com/download))
- **Azure OpenAI Service** access with API keys
  - Or OpenAI API keys (with minor code modifications)
- **Visual Studio 2022**, **VS Code**, or **Rider** (optional)

### Installation

1. **Clone the repository**:
   ```bash
   git clone https://github.com/melvinsewak/portfolio_microsoft_agent_framework.git
   cd portfolio_microsoft_agent_framework
   ```

2. **Restore NuGet packages**:
   ```bash
   dotnet restore
   ```

3. **Configure your API keys** (choose one method):

   **Option A: Environment Variables** (Recommended for development)
   ```bash
   export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"
   export AZURE_OPENAI_API_KEY="your-api-key"
   export AZURE_OPENAI_DEPLOYMENT="gpt-4"
   ```

   **Option B: Configuration File**
   
   Copy the template and fill in your values:
   ```bash
   cd examples/01-BasicChatbot
   cp appsettings.template.json appsettings.json
   # Edit appsettings.json with your credentials
   ```

### Running the Examples

Each example can be run independently:

```bash
# Example 1: Basic Chatbot
cd examples/01-BasicChatbot
dotnet run

# Example 2: Single Agent with Tools
cd examples/02-SingleAgentWithTools
dotnet run

# Example 3: Multi-Agent Orchestrator
cd examples/03-MultiAgentOrchestrator
dotnet run

# Example 4: Advanced Patterns
cd examples/04-AdvancedPatterns
dotnet run
```

**Note**: All examples include a **demo mode** that runs without API keys to showcase the structure and patterns.

## 📦 Dependencies

All examples use the latest compatible packages:

- **Microsoft.Extensions.AI** (10.3.0+) - Core AI abstractions
- **Microsoft.Extensions.AI.OpenAI** (10.3.0+) - OpenAI integration
- **Azure.AI.OpenAI** (2.1.0+) - Azure OpenAI client
- **Microsoft.Extensions.Configuration** - Configuration management

## 🏗️ Architecture & Design Patterns

### Single Agent Pattern
```
User Input → AI Agent → Response
              ↓
         (Tools/Functions)
```
Best for: Simple chatbots, Q&A systems, single-purpose assistants

### Multi-Agent Pattern (Orchestrator)
```
User Input → Orchestrator → Specialized Agent 1
                    ↓ ─────→ Specialized Agent 2
                    ↓ ─────→ Specialized Agent 3
                    ↓
              Aggregated Response
```
Best for: Complex workflows, domain-specific expertise, task delegation

### Agent with Tools Pattern
```
User Input → AI Agent → Tool Selection → Tool Execution → Response
                ↓
         [Calculator, Weather, Tasks, ...]
```
Best for: Action-oriented agents, API integrations, data manipulation

## 🔐 Security Best Practices

1. **Never commit API keys** to version control
2. Use **environment variables** or **Azure Key Vault** for secrets
3. Implement **rate limiting** and **quota management**
4. Add **input validation** and **output sanitization**
5. Use **Azure Managed Identity** in production environments
6. Enable **logging and monitoring** for audit trails

## 🧪 Testing

Build and verify all examples:

```bash
# Build entire solution
dotnet build

# Run all examples in demo mode (no API keys required)
cd examples/01-BasicChatbot && dotnet run && cd ../..
cd examples/02-SingleAgentWithTools && dotnet run && cd ../..
cd examples/03-MultiAgentOrchestrator && dotnet run && cd ../..
cd examples/04-AdvancedPatterns && dotnet run && cd ../..
```

## 📖 Additional Resources

### Official Documentation
- [Microsoft Agent Framework Overview](https://learn.microsoft.com/en-us/agent-framework/overview/)
- [Microsoft.Extensions.AI Documentation](https://learn.microsoft.com/en-us/dotnet/ai/ai-extensions-overview)
- [Azure OpenAI Service](https://learn.microsoft.com/en-us/azure/ai-services/openai/)

### Sample Repositories
- [Microsoft Agent Framework Samples](https://github.com/microsoft/Agent-Framework-Samples)
- [.NET AI Samples](https://github.com/dotnet/ai-samples)

### Tutorials & Guides
- [Building Your First AI Agent in C#](https://dev.to/matteo_davena/building-your-first-ai-agent-in-c-with-microsoft-agent-framework-i33)
- [AI Agents for Beginners](https://microsoft.github.io/ai-agents-for-beginners/)

## 🤝 Contributing

Contributions are welcome! If you have additional patterns or improvements:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-pattern`)
3. Commit your changes (`git commit -m 'Add amazing pattern'`)
4. Push to the branch (`git push origin feature/amazing-pattern`)
5. Open a Pull Request

## 📝 License

This project is provided as-is for educational and demonstration purposes.

## 💬 Support

For questions or issues:
- Open an issue in this repository
- Refer to the [official Microsoft documentation](https://learn.microsoft.com/en-us/agent-framework/)
- Check the [Microsoft Q&A forum](https://learn.microsoft.com/en-us/answers/)

## 🎓 Learning Path

**Recommended order for learning**:
1. Start with **Example 1 (Basic Chatbot)** to understand core concepts
2. Move to **Example 2 (Tools)** to learn function calling
3. Explore **Example 3 (Multi-Agent)** for orchestration patterns
4. Study **Example 4 (Advanced)** for production patterns

## 🔄 Version Compatibility

- **.NET**: 8.0 or later (all examples target `net8.0`)
- **Microsoft.Extensions.AI**: 10.3.0 or later
- **Azure.AI.OpenAI**: 2.1.0 or later
- Compatible with both **Azure OpenAI** and **OpenAI API**

## 🌟 Key Features Demonstrated

- ✅ Single and Multi-Agent patterns
- ✅ Function/Tool calling
- ✅ Conversation memory and context
- ✅ Streaming responses
- ✅ Error handling and resilience
- ✅ Observability and monitoring
- ✅ Production-ready configuration
- ✅ Demo mode for testing without API keys

---

**Built with ❤️ using Microsoft Agent Framework and .NET**