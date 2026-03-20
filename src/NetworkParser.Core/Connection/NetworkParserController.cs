using System.Net.NetworkInformation;
using NetworkParser.Core.Abstractions.Connection;
using NetworkParser.Domain.Interfaces;
using NetworkParser.Domain.Packets;
using PacketDotNet;
using SharpPcap;
using SharpPcap.LibPcap;

namespace NetworkParser.Core.Connection; 
public class NetworkParserController : INetworkParserController {
    private ICaptureDevice? device;
    private int packetCounter = 0;
    private readonly List<PacketModel> packets = new();

    public event Action<PacketModel>? PacketCaptured;
    /// <summary>
    /// Получение списка сетевых интерфейсов (для UI)
    /// </summary>
    /// <returns></returns>
    public List<NetworkInterfaceModel> GetAvailableInterfaces () {
        var devices = LibPcapLiveDeviceList.Instance;
        var models = new List<NetworkInterfaceModel>();
        for (int i = 0; i < devices.Count; i++) {
            var dev = devices[i];
            var sysInterface = NetworkInterface.GetAllNetworkInterfaces()
                .FirstOrDefault(ni => dev.Name.Contains(ni.Id));

            var model = new NetworkInterfaceModel {
                Index = i,
                Name = dev.Name,
                Description = dev.Description ?? "No description",
                FriendlyName = dev.Interface?.FriendlyName ?? dev.Description ?? dev.Name,
                MacAddress = dev.MacAddress?.ToString() ?? "N/A",
                IsUp = sysInterface?.OperationalStatus == OperationalStatus.Up,
                IpAddress = dev.Addresses?
                                .FirstOrDefault(
                                    a => a.Addr?.ipAddress?.AddressFamily 
                                    == System.Net.Sockets.AddressFamily.InterNetwork)?
                                .Addr?.ToString()
                                ?? "No IP"
            };
            model.ConnectionType = GuessConnectionType(dev);
            models.Add(model);
        }
        return models;
    }

    /// <summary>
    ///  Запуск захвата
    /// </summary>
    /// <param name="deviceIndex"></param>
    /// <param name="bpfFilter"></param>
    /// <exception cref="ArgumentException"></exception>
    public void StartCapture (int deviceIndex = 0, string? bpfFilter = null) {
        if (device != null){ StopCapture(); }
        var devices = LibPcapLiveDeviceList.Instance;
        if (deviceIndex < 0 || deviceIndex >= devices.Count){
            throw new ArgumentException("Неверный индекс сетевого устройства");
        }
        for (int i = 0; i < devices.Count; i++) {
            System.Diagnostics.Debug.WriteLine($"{i}: {devices[i].Description}");
        }
        device = devices[deviceIndex];
        device.OnPacketArrival += Device_OnPacketArrival;
        device.Open(DeviceModes.Promiscuous | DeviceModes.MaxResponsiveness, 1000);
        if (!string.IsNullOrEmpty(bpfFilter)){
            device.Filter = bpfFilter;
        }
        device.StartCapture();
        packetCounter = 0;
    }

    /// <summary>
    /// Остановка
    /// </summary>
    public void StopCapture () {
        device?.StopCapture();
        device?.Close();
        device = null;
    }

    /// <summary>
    /// Применение фильтра
    /// </summary>
    /// <param name="filter"></param>
    public void ApplyFilter (string filter) {
        if (device?.Started == true){
            device.Filter = filter;
        }
    }

    /// <summary>
    /// Отправка пакета (спуфинг/injection)
    /// </summary>
    /// <param name="rawData"></param>
    public void SendPacket (byte[] rawData) {
        if (device is SharpPcap.IInjectionDevice injectionDevice) {
            injectionDevice.SendPacket(rawData);
        }
    }
    private string GuessConnectionType (SharpPcap.ICaptureDevice dev) {
        if (dev.Name.Contains("loopback", StringComparison.OrdinalIgnoreCase)){
            return "Loopback";
        }
        if (dev.Name.Contains("wlan") || dev.Description?.Contains("Wireless") == true){
            return "Wi-Fi";
        }
        if (dev.Name.Contains("eth") || dev.Description?.Contains("Ethernet") == true){
            return "Ethernet";
        }
        if (dev.Name.Contains("tun") || dev.Name.Contains("tap")){
            return "VPN/Tunnel";
        }
        return "Unknown";
    }
    private void Device_OnPacketArrival (object sender, PacketCapture e) {
        System.Diagnostics.Debug.WriteLine("PACKET ARRIVED");
        var rawPacket = e.GetPacket();
        var parsed = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);
        var model = new PacketModel {
            Number = ++packetCounter,
            Timestamp = DateTime.UtcNow,
            Length = rawPacket.Data.Length,
            RawData = rawPacket.Data.ToArray(),
            Info = parsed.ToString() 
        };

        // Заполняем основные поля
        if (parsed is EthernetPacket eth) {
            model.Source = eth.SourceHardwareAddress?.ToString() ?? "N/A";
            model.Destination = eth.DestinationHardwareAddress?.ToString() ?? "N/A";
            model.Protocol = eth.Type.ToString();

            if (eth.PayloadPacket is IPPacket ip) {
                model.Source = ip.SourceAddress.ToString();
                model.Destination = ip.DestinationAddress.ToString();
                model.Protocol = ip.Protocol.ToString();
            }
        }

        packets.Add(model);
        PacketCaptured?.Invoke(model);
    }
}
