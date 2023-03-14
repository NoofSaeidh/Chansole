using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI_API;

namespace Chansole.Services;

public interface IChatGptService
{
    Task<bool> Validate();
}

public class ChatGptOptions
{
    public string? SecurityToken {get; init;}
}

internal class ChatGptService : IChatGptService
{
    private readonly ILogger<ChatGptService> _logger;
    private readonly OpenAIAPI _client;

    public ChatGptService(IOptions<ChatGptOptions> options, ILogger<ChatGptService> logger)
    {
        _logger = logger;
        _client = new OpenAIAPI(new APIAuthentication(options.Value.SecurityToken));
    }

    public async Task<bool> Validate()
    {
        return await _client.Auth.ValidateAPIKey();
    }
}