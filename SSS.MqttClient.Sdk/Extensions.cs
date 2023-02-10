using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using System.Text;

namespace SSS.MqttClient.Sdk
{
    public static class Extensions
    {
        public static Dictionary<string, object> Parse(this byte[] bytes)
        {
            if (bytes is null) return default!;

            string jsonStr = Encoding.UTF8.GetString(bytes);

            return JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonStr)!;
        }

        public static MqttClientOptions GenerateMqttClientOptions(this MqttFactory mqttFactory, string host, string username, string password)
        {
            return new MqttClientOptionsBuilder()
                        .WithClientId(username)
                        .WithTcpServer(server: host)
                        .WithCredentials(username, password)
                        .Build();
        }

        public static async Task DisconnectServerAsync(this MqttFactory mqttFactory, IMqttClient mqttClient)
        {
            var mqttClientDisconnectOptions = mqttFactory.CreateClientDisconnectOptionsBuilder().Build();

            await mqttClient.DisconnectAsync(mqttClientDisconnectOptions, CancellationToken.None);
        }
    }
}
