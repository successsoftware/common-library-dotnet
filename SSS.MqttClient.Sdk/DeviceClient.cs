using Microsoft.Extensions.Logging;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.Rpc;
using MQTTnet.Protocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using SSS.MqttClient.Sdk.Models;

namespace SSS.MqttClient.Sdk
{
    public class DeviceClient
    {
        private readonly ILogger<DeviceClient> _logger;
        private readonly MqttFactory _mqttFactory;
        private readonly MqttClientOptions _mqttClientOptions;

        public DeviceClient(MqttFactory mqttFactory,
            MqttClientOptions mqttClientOptions,
            ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DeviceClient>();
            _mqttFactory = mqttFactory;
            _mqttClientOptions = mqttClientOptions;
        }

        public async Task<MqttClientPublishResult> PublishMessageToTopic<T>(T data, string topic, MqttClientOptions mqttClientOptions = null)
            where T : class, IKafkaMessage
        {
            using var mqttClient = _mqttFactory.CreateMqttClient();

            await ConnectAsync(mqttClient, mqttClientOptions ?? _mqttClientOptions);

            var payload = JsonConvert.SerializeObject(data, new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });

            var message = _mqttFactory.CreateApplicationMessageBuilder()
                                            .WithTopic(topic)
                                            .WithPayload(payload)
                                            .WithContentType("application/json")
                                            .Build();

            var result = await mqttClient.PublishAsync(message);

            await _mqttFactory.DisconnectServerAsync(mqttClient);

            return result;
        }

        public async Task PublishMessageRpc<T>(T data, string methodName, int timeout = 2, MqttClientOptions mqttClientOptions = null)
            where T : class, IKafkaMessage
        {
            using var mqttClient = _mqttFactory.CreateMqttClient();

            await ConnectAsync(mqttClient, mqttClientOptions ?? _mqttClientOptions);

            using var mqttRpcClient = _mqttFactory.CreateMqttRpcClient(mqttClient);

            var payload = JsonConvert.SerializeObject(data, new JsonSerializerSettings()
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            });
            // Access to a fully featured application message is not supported for RPC calls!
            // The method will throw an exception when the response was not received in time.
            await mqttRpcClient.ExecuteAsync(TimeSpan.FromSeconds(timeout), methodName, payload, MqttQualityOfServiceLevel.AtMostOnce);

            await _mqttFactory.DisconnectServerAsync(mqttClient);
        }
        /*
         * The device must respond to the request using the correct topic. The following C code shows how a
         * smart device like an ESP8266 must respond to the above sample.
         *
            // If using the MQTT client PubSubClient it must be ensured 
            // that the request topic for each method is subscribed like the following.
            mqttClient.subscribe("MQTTnet.RPC/+/ping");
            mqttClient.subscribe("MQTTnet.RPC/+/do_something");

            // It is not allowed to change the structure of the topic.
            // Otherwise RPC will not work.
            // So method names can be separated using an _ or . but no +, # or /.
            // If it is required to distinguish between devices
            // own rules can be defined like the following:
            mqttClient.subscribe("MQTTnet.RPC/+/deviceA.ping");
            mqttClient.subscribe("MQTTnet.RPC/+/deviceB.ping");
            mqttClient.subscribe("MQTTnet.RPC/+/deviceC.getTemperature");

            // Within the callback of the MQTT client the topic must be checked
            // if it belongs to MQTTnet RPC. The following code shows one
            // possible way of doing this.
            void mqtt_Callback(char *topic, byte *payload, unsigned int payloadLength)
            {
	            String topicString = String(topic);

	            if (topicString.startsWith("MQTTnet.RPC/")) {
		            String responseTopic = topicString + String("/response");

		            if (topicString.endsWith("/deviceA.ping")) {
			            mqtt_publish(responseTopic, "pong", false);
			            return;
		            }
	            }
            }

            // Important notes:
            // ! Do not send response message with the _retain_ flag set to true.
            // ! All required data for a RPC call and the result must be placed into the payload.
         */

        private async Task ConnectAsync(IMqttClient mqttClient, MqttClientOptions mqttClientOptions)
        {
            var response = await mqttClient.ConnectAsync(mqttClientOptions);

            if (response.ResultCode != MqttClientConnectResultCode.Success) throw new ArgumentException("Fail to connect MQTT server.");

            _logger.LogInformation("The MQTT client is connected.");
        }
    }
}
