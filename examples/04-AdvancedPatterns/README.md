# Example 4: Advanced Agent Patterns

## Overview
This example demonstrates production-ready patterns and best practices for building robust AI agent applications:
- **Streaming Responses**: Real-time token-by-token generation
- **Conversation Memory Management**: Token limit handling and context pruning
- **Error Handling & Resilience**: Retry logic with exponential backoff
- **Observability**: Logging, metrics, and monitoring

## Pattern 1: Streaming Responses

### Why Streaming?
Streaming provides better user experience by showing responses as they're generated rather than waiting for completion.

**Benefits:**
- Lower perceived latency
- Progressive disclosure of information
- Better for long responses
- Ability to cancel long-running operations

### Implementation

```csharp
await foreach (var update in chatClient.GetStreamingResponseAsync(messages))
{
    foreach (var content in update.Contents)
    {
        if (content is TextContent textContent)
        {
            Console.Write(textContent.Text);
        }
    }
}
```

### Use Cases
- Chat interfaces
- Real-time translation
- Code generation
- Long-form content creation

## Pattern 2: Conversation Memory Management

### The Challenge
- LLMs have token limits (e.g., 8K, 16K, 128K tokens)
- Conversations can exceed these limits
- Need to maintain relevant context while staying within limits

### Solution: ConversationMemoryManager

```csharp
public class ConversationMemoryManager
{
    private readonly List<ChatMessage> _messages = new();
    private readonly int _maxTokens;

    public void AddMessage(ChatRole role, string content)
    {
        _messages.Add(new ChatMessage(role, content));
        PruneIfNeeded(); // Automatically remove old messages
    }

    public int EstimateTokenCount()
    {
        // Rough estimation: ~4 characters per token
        return _messages.Sum(m => m.Text?.Length ?? 0) / 4;
    }

    private void PruneIfNeeded()
    {
        while (EstimateTokenCount() > _maxTokens && _messages.Count > 1)
        {
            _messages.RemoveAt(0); // Remove oldest messages
        }
    }
}
```

### Strategies

#### 1. Sliding Window
Keep only the N most recent messages:
```csharp
while (_messages.Count > MAX_MESSAGES)
{
    _messages.RemoveAt(0);
}
```

#### 2. Token-Based Pruning
Remove old messages when approaching token limit (shown above)

#### 3. Summarization
Periodically summarize old context:
```csharp
if (EstimateTokenCount() > THRESHOLD)
{
    var summary = await SummarizeOldContext();
    _messages.Clear();
    _messages.Add(new ChatMessage(ChatRole.System, summary));
}
```

#### 4. Importance-Based Retention
Keep important messages (e.g., system prompts, key information)

## Pattern 3: Error Handling & Resilience

### The Challenge
- Network failures
- Rate limiting
- Service outages
- Timeout errors

### Solution: Retry with Exponential Backoff

```csharp
public class ResilientChatClient
{
    private readonly IChatClient _innerClient;
    private readonly int _maxRetries = 3;

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
                var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt));
                await Task.Delay(delay);
            }
        }
        throw new Exception("Max retries exceeded");
    }
}
```

### Retry Strategies

#### Exponential Backoff
```
Attempt 1: Immediate
Attempt 2: Wait 2 seconds
Attempt 3: Wait 4 seconds
Attempt 4: Wait 8 seconds
```

#### Jittered Backoff
Add randomness to prevent thundering herd:
```csharp
var delay = TimeSpan.FromSeconds(Math.Pow(2, attempt) * (0.5 + Random.Shared.NextDouble()));
```

### Circuit Breaker Pattern
Prevent cascading failures:
```csharp
public class CircuitBreaker
{
    private int _failureCount = 0;
    private DateTime _lastFailure;
    private const int THRESHOLD = 5;
    private const int COOLDOWN_SECONDS = 60;

    public bool IsOpen()
    {
        if (_failureCount >= THRESHOLD)
        {
            if ((DateTime.UtcNow - _lastFailure).TotalSeconds < COOLDOWN_SECONDS)
            {
                return true; // Circuit is open
            }
            // Reset after cooldown
            _failureCount = 0;
        }
        return false;
    }

    public void RecordFailure()
    {
        _failureCount++;
        _lastFailure = DateTime.UtcNow;
    }

    public void RecordSuccess()
    {
        _failureCount = 0;
    }
}
```

## Pattern 4: Observability

### Why Observability?
- Debugging issues
- Performance monitoring
- Usage tracking
- Cost management
- Quality assurance

### ObservableChatClient

```csharp
public class ObservableChatClient
{
    private readonly IChatClient _innerClient;
    private readonly List<RequestMetric> _metrics = new();

    public async Task<string> CompleteWithLoggingAsync(List<ChatMessage> messages)
    {
        var requestId = Guid.NewGuid();
        var startTime = DateTime.UtcNow;

        Console.WriteLine($"📤 Request {requestId}: {messages.Last().Text}");

        try
        {
            var response = await _innerClient.GetResponseAsync(messages);
            var duration = DateTime.UtcNow - startTime;

            _metrics.Add(new RequestMetric
            {
                RequestId = requestId,
                Success = true,
                Duration = duration,
                TokenCount = EstimateTokens(response.Text)
            });

            Console.WriteLine($"📥 Response {requestId}: Success ({duration.TotalMilliseconds}ms)");
            return response.Text ?? "";
        }
        catch (Exception ex)
        {
            _metrics.Add(new RequestMetric
            {
                RequestId = requestId,
                Success = false,
                Error = ex.Message
            });
            throw;
        }
    }
}
```

### Metrics to Track

#### Performance Metrics
- Response time (P50, P95, P99)
- Throughput (requests per second)
- Error rate

#### Cost Metrics
- Total tokens used
- Cost per request
- Cost per user/session

#### Quality Metrics
- User satisfaction ratings
- Response accuracy
- Tool invocation success rate

### Logging Best Practices

#### Structured Logging
```csharp
logger.LogInformation(
    "Agent request completed. RequestId={RequestId}, Duration={Duration}ms, Tokens={Tokens}",
    requestId,
    duration.TotalMilliseconds,
    tokenCount
);
```

#### Correlation IDs
Track requests across system boundaries:
```csharp
public class CorrelationContext
{
    public string CorrelationId { get; set; }
    public string UserId { get; set; }
    public string SessionId { get; set; }
}
```

### Monitoring Dashboard Example

```
┌─────────────────────────────────────────┐
│     Agent Performance Dashboard         │
├─────────────────────────────────────────┤
│ Total Requests: 1,234                   │
│ Success Rate:   98.5%                   │
│ Avg Latency:    1.2s                    │
│ P95 Latency:    3.5s                    │
│ Total Tokens:   45,678                  │
│ Estimated Cost: $12.34                  │
└─────────────────────────────────────────┘
```

## Running the Example

### Prerequisites
1. .NET 10.0 SDK or later
2. Azure OpenAI service (optional for demo mode)

### Configuration
```bash
export AZURE_OPENAI_ENDPOINT="https://your-resource.openai.azure.com/"
export AZURE_OPENAI_API_KEY="your-api-key"
```

### Run
```bash
dotnet run
```

### Menu Options
1. Streaming Responses Demo
2. Conversation Memory Management Demo
3. Error Handling & Resilience Demo
4. Agent Observability Demo
5. Run all demos

## Production Checklist

### ✅ Performance
- [ ] Implement streaming for better UX
- [ ] Add response caching where appropriate
- [ ] Use async/await consistently
- [ ] Optimize token usage

### ✅ Reliability
- [ ] Implement retry logic with exponential backoff
- [ ] Add circuit breakers for external services
- [ ] Handle rate limiting gracefully
- [ ] Set appropriate timeouts

### ✅ Observability
- [ ] Log all requests and responses
- [ ] Track performance metrics
- [ ] Monitor error rates
- [ ] Set up alerts for anomalies

### ✅ Security
- [ ] Validate all inputs
- [ ] Sanitize outputs
- [ ] Use secure credential storage
- [ ] Implement rate limiting per user

### ✅ Cost Management
- [ ] Track token usage
- [ ] Set spending limits
- [ ] Implement token budgets per user
- [ ] Monitor and optimize prompt sizes

## Integration with Monitoring Tools

### Application Insights
```csharp
services.AddApplicationInsightsTelemetry();

// Track custom metrics
telemetryClient.TrackMetric("AgentResponseTime", duration.TotalMilliseconds);
telemetryClient.TrackMetric("TokensUsed", tokenCount);
```

### OpenTelemetry
```csharp
using var tracerProvider = Sdk.CreateTracerProviderBuilder()
    .AddSource("AgentApp")
    .AddConsoleExporter()
    .Build();

using var activity = activitySource.StartActivity("ChatCompletion");
activity?.SetTag("user.id", userId);
activity?.SetTag("model", modelName);
```

### Prometheus
```csharp
private static readonly Counter RequestCounter = Metrics
    .CreateCounter("agent_requests_total", "Total agent requests");

private static readonly Histogram RequestDuration = Metrics
    .CreateHistogram("agent_request_duration_seconds", "Agent request duration");

RequestCounter.Inc();
using (RequestDuration.NewTimer())
{
    await chatClient.GetResponseAsync(messages);
}
```

## Next Steps

- Apply these patterns to your production agents
- Integrate with your existing monitoring infrastructure
- Customize metrics and logging for your use case
- Build dashboards for operational visibility

## Related Resources

- [.NET Application Insights](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview)
- [OpenTelemetry for .NET](https://opentelemetry.io/docs/languages/net/)
- [Polly Resilience Library](https://github.com/App-vNext/Polly)
- [Azure Monitor](https://learn.microsoft.com/en-us/azure/azure-monitor/)
