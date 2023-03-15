using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Reflection;

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

    public ConsoleHostedService(IChatGptService chatGpt, ConsoleArguments arguments, IAnsiConsole console,
                                IHostApplicationLifetime applicationLifetime)
    {
        _chatGpt             = chatGpt;
        _arguments           = arguments;
        _console             = console;
        _applicationLifetime = applicationLifetime;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        //var validate = new Option<bool>("validate", "Validate ChatGPT is working and api token is correct");
        //var root     = new RootCommand("ChatGPT console") {validate};
        //root.SetHandler(async (validate) =>
        //{
        //    if (validate)
        //    {
        //        var res = await _chatGpt.Validate(stoppingToken);
        //        _console.WriteLine($"Validation {(res ? "succeed" : "failed")}.",
        //                           new Style(res ? null : Color.Red, decoration: Decoration.Bold));
        //    }

        //    _applicationLifetime.StopApplication();
        //}, validate);
        //await root.InvokeAsync(_arguments.Args);

        var root = new RootCommand("ChatGPT console");

        var validate = new Command("validate", "Validate ChatGPT is working and api token is correct");
        root.AddCommand(validate);
        validate.SetHandler(async (ctx) =>
        {
            var res = await _chatGpt.Validate(ctx.GetCancellationToken());
            _console.WriteLine($"Validation {(res ? "succeed" : "failed")}.",
                               new Style(res ? null : Color.Red, decoration: Decoration.Bold));
        });


        //root.SetHandler(async (validate) =>
        //{
        //    if (validate)
        //    {
        //        var res = await _chatGpt.Validate(stoppingToken);
        //        _console.WriteLine($"Validation {(res ? "succeed" : "failed")}.",
        //                           new Style(res ? null : Color.Red, decoration: Decoration.Bold));
        //    }
        //});
        var startupOrder = (MiddlewareOrder)(-4000); // internalvalue
        var consoleBuilder = new CommandLineBuilder(root)
                             .UseDefaults()
                             .AddMiddleware(async (context, next) =>
                             {
                                 var addMethod = context.GetType()
                                                        .GetEvent("CancellationHandlingAdded",
                                                                  BindingFlags.Instance | BindingFlags.NonPublic)
                                                        !.GetAddMethod(true);
                                 addMethod!.Invoke(context, new object[]
                                 {
                                     (CancellationTokenSource source) =>
                                     {
                                         stoppingToken.Register(() => source!.Cancel());
                                     }
                                 });
                                 await next(context);
                             }, startupOrder)
                             .AddMiddleware(async (context, next) =>
                             {
                                 try
                                 {
                                     await next(context);
                                 }
                                 finally
                                 {
                                     _applicationLifetime.StopApplication();
                                 }
                             }, startupOrder)
                             .Build();

        await consoleBuilder.InvokeAsync(_arguments.Args);
    }
}