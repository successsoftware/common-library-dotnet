using Demo.MqttClient;
using Microsoft.AspNetCore.Hosting;
using SSS.MqttClient.Sdk;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureHostConfiguration(configHost =>
    {
        configHost.SetBasePath(Directory.GetCurrentDirectory());
        configHost.AddJsonFile("appsettings.json", optional: true)
                  .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")}.json", optional: true);
        configHost.AddEnvironmentVariables(prefix: "ASPNETCORE_");
    })
    .ConfigureServices((context, services) =>
    {
        services.AddHostedService<Worker>();

        services.AddMqttWrapper(context.Configuration);
    })
    .Build();

host.Run();
