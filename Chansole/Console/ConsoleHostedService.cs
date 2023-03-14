using Chansole.Services;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace Chansole.Console;


internal class ConsoleHostedService : BackgroundService
{
    private readonly IChatGptService _chatGpt;
    private readonly ConsoleArguments _arguments;

    public ConsoleHostedService(IChatGptService chatGpt, ConsoleArguments arguments)
    {
        _chatGpt = chatGpt;
        _arguments = arguments;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var res = await _chatGpt.Validate();
        AnsiConsole.WriteLine($"Validation {(res ? "succeed" : "failed")}");
    }
}