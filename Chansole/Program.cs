using Autofac.Extensions.DependencyInjection;
using Chansole;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;


await Host.CreateDefaultBuilder(args)
          .UseServiceProviderFactory(new AutofacServiceProviderFactory())
          .ConfigureServices()
          .ConfigureAppConfiguration(config => config.AddUserSecrets<Program>())
          .Build()
          .RunAsync();