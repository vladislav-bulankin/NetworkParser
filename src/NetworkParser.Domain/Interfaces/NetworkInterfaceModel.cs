namespace NetworkParser.Domain.Interfaces;

public class NetworkInterfaceModel {

        /// <summary>
        /// Индекс в списке SharpPcap (LibPcapLiveDeviceList.Instance[index])
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Техническое имя устройства (например: \Device\NPF_{GUID})
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Человеко-понятное имя (FriendlyName) — самое важное для UI
        /// </summary>
        public string FriendlyName { get; set; } = string.Empty;

        /// <summary>
        /// Описание (часто совпадает с FriendlyName или более детальное)
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// MAC-адрес (физический адрес адаптера)
        /// </summary>
        public string? MacAddress { get; set; }

        /// <summary>
        /// Основной IP-адрес (первый из IPv4, если несколько)
        /// </summary>
        public string? IpAddress { get; set; }

        /// <summary>
        /// Статус — работает ли интерфейс (up/down)
        /// </summary>
        public bool IsUp { get; set; }

        /// <summary>
        /// Тип соединения (Ethernet, Wi-Fi, Loopback, VPN и т.д.) — можно вычислить
        /// </summary>
        public string? ConnectionType { get; set; }

        /// <summary>
        /// Скорость интерфейса (если доступна, в Mbps)
        /// </summary>
        public long? Speed { get; set; }

        /// <summary>
        /// Для отображения в списке — переопределим ToString()
        /// </summary>
        public override string ToString () {
            var parts = new List<string>();

            if (!string.IsNullOrEmpty(FriendlyName)){
                parts.Add(FriendlyName);
            }
            else if (!string.IsNullOrEmpty(Description)){
                parts.Add(Description);
            }
            else{
                parts.Add(Name);
            }

            if (!string.IsNullOrEmpty(IpAddress)){
                parts.Add($"({IpAddress})");
            }

            if (MacAddress != null){
                parts.Add($"MAC: {MacAddress}");
            }

            if (IsUp){
                parts.Add("Up");
            }
            else{
                parts.Add("Down");
            }

            return string.Join(" • ", parts);
        }
}

