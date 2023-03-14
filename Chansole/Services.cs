using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog.Debugging;
using Serilog;
using Autofac;

namespace Chansole;

public static class Services
{
    public static IHostBuilder ConfigureServices(this IHostBuilder hostBuilder)
    {
        hostBuilder.ConfigureServices((host, services) =>
        {
            Registrar.Register(services, host.Configuration);
        });
        hostBuilder.ConfigureContainer<ContainerBuilder>(builder =>
        {
            builder.RegisterModule<Module>();
        });
        return hostBuilder;
    }

    private class Registrar
    {
        private readonly IServiceCollection _services;
        private readonly IConfiguration _configuration;

        private Registrar(IServiceCollection services, IConfiguration configuration)
        {
            _services      = services;
            _configuration = configuration;
        }

        public static void Register(IServiceCollection services, IConfiguration configuration)
        {
            var registrar = new Registrar(services, configuration);
            registrar.RegisterLogger();
            registrar.RegisterHostedServices();
        }

        private void RegisterLogger()
        {
            const string path = ".\\logs\\";

            Directory.CreateDirectory(path: path);

            Log.Logger = new LoggerConfiguration()
                         .ReadFrom.Configuration(_configuration)
                         .CreateLogger();

            var selfLog = File.CreateText(path + "self-log.txt");
            SelfLog.Enable(TextWriter.Synchronized(writer: selfLog));

            _services.AddSingleton(Log.Logger)
                     .AddLogging(config =>
                     {
                         config.ClearProviders();
                         config.AddSerilog(Log.Logger, dispose: true);
                     });
        }

        private void RegisterHostedServices()
        {
            _services.AddHostedService<ConsoleHostedService>();
        }
    }

    private class Module : Autofac.Module
    {

    }
}