using Autofac.Extensions.DependencyInjection;
using Chansole.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;


await Host.CreateDefaultBuilder(args)
          .UseServiceProviderFactory(new AutofacServiceProviderFactory())
          .ConfigureServices(args)
          .ConfigureAppConfiguration(config => config.AddUserSecrets<Program>())
          .Build()
          .RunAsync();