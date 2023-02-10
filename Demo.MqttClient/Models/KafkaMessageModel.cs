using SSS.MqttClient.Sdk.Models;

namespace Demo.MqttClient.Models
{
    public class KafkaMessageModel : IKafkaMessage
    {
        public string GatewayNo { get; private set; }
        public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;
        public string Type { get; set; } = "init";
        public List<DeviceData> Data { get; set; } = new();

        public KafkaMessageModel(string gwNo)
        {
            GatewayNo = gwNo;
        }
    }
}
