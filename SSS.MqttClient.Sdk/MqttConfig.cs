using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MQTTnet;
using MQTTnet.Client;
using SSS.MqttClient.Sdk.Models;

namespace SSS.MqttClient.Sdk
{
    public static class MqttConfig
    {
        public static IServiceCollection AddMqttWrapper(this IServiceCollection services, IConfiguration configuration)
        {
            MqttClientSettings mqttClientSettings = new();

            configuration.GetSection(MqttClientSettings.Name).Bind(mqttClientSettings);

            services.AddSingleton(sp =>
                    {
                        var mqttClientOptions = new MqttClientOptionsBuilder()
                            .WithClientId(mqttClientSettings.Username)
                            .WithTcpServer(server: mqttClientSettings.Host)
                            .WithCredentials(mqttClientSettings.Username, mqttClientSettings.Password)
                            .Build();
                        return mqttClientOptions;
                    });

            services.AddSingleton<MqttFactory>();

            services.AddScoped<DeviceClient>();

            return services;
        }
    }
}
