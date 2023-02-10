using Demo.MqttClient.Models;
using SSS.MqttClient.Sdk;

namespace Demo.MqttClient
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IDeviceClient _deviceClient;

        public Worker(ILogger<Worker> logger, IDeviceClient deviceClient)
        {
            _logger = logger;
            _deviceClient = deviceClient;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await _deviceClient.ConnectServerAsync();

            await base.StartAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));

            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                _logger.LogInformation("Worker running at: {time}", DateTime.UtcNow);

                var data = new KafkaMessageModel("GW02")
                {
                    Data = new List<DeviceData>{
                        new DeviceData
                        {
                            ModelNo = "SRMD01",
                            SerialNo = "SRDE01",
                            OnOff = "On",
                            BatteryPercentage = 100,
                            SpeedSetting = "LOW"
                        }
                    },
                    Type = "telemetry"
                };

                await _deviceClient.PublishMessageToTopic(data, "mqtt_topic");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _deviceClient.DisconnectServerAsync();

            await base.StopAsync(cancellationToken);
        }
    }
}