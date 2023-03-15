using System.CommandLine;
using Chansole.Services;
using Microsoft.Extensions.Hosting;
using Spectre.Console;

namespace Chansole.Console;

internal class ConsoleHostedService : BackgroundService
{
    private readonly IChatGptService _chatGpt;
    private readonly ConsoleArguments _arguments;
    private readonly IAnsiConsole _console;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public ConsoleHostedService(IChatGptService chatGpt, ConsoleArguments arguments, IAnsiConsole console, IHostApplicationLifetime applicationLifetime)
    {
        _chatGpt   = chatGpt;
        _arguments = arguments;
        _console   = console;
        _applicationLifetime = applicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var validate = new Option<bool>("validate", "Validate ChatGPT is working and api token is correct");
        var root     = new RootCommand("ChatGPT console") {validate};
        root.SetHandler(async (validate) =>
        {
            if (validate)
            {
                var res = await _chatGpt.Validate(stoppingToken);
                _console.WriteLine($"Validation {(res ? "succeed" : "failed")}.",
                                   new Style(res ? null : Color.Red, decoration: Decoration.Bold));
            }

            _applicationLifetime.StopApplication();
        }, validate);
        await root.InvokeAsync(_arguments.Args);
    }
}