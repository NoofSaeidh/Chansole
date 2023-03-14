using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenAI_API;

namespace Chansole;

public interface IChatGptService
{
}

public record ChatGptOptions(string SecurityToken)
{
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
        using(var log = _logger.BeginScope)
        return await _client.Auth.ValidateAPIKey();
    }
}