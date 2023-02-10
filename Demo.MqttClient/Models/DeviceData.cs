namespace Demo.MqttClient.Models
{
    public class DeviceData
    {
        public string ModelNo { get; set; } = "";
        public string SerialNo { get; set; } = "";
        public string OnOff { get; set; } = "";
        public int BatteryPercentage { get; set; } = 0;
        public string SpeedSetting { get; set; } = "";
    }
}
