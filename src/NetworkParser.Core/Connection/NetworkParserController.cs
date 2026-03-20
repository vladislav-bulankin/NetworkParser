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
        var rawPacket = e.GetPacket();
        var parsed = Packet.ParsePacket(rawPacket.LinkLayerType, rawPacket.Data);

        var model = new PacketModel {
            Number = ++packetCounter,
            Timestamp = DateTime.Now,
            Length = rawPacket.Data.Length,
            RawData = rawPacket.Data.ToArray(),
        };

        if (parsed is EthernetPacket eth) {
            model.Source = eth.SourceHardwareAddress?.ToString() ?? "N/A";
            model.Destination = eth.DestinationHardwareAddress?.ToString() ?? "N/A";
            model.Protocol = eth.Type.ToString();

            if (eth.PayloadPacket is IPPacket ip) {
                model.Source = ip.SourceAddress.ToString();
                model.Destination = ip.DestinationAddress.ToString();

                if (ip.PayloadPacket is TcpPacket tcp) {
                    model.Protocol = "Tcp";
                    model.Info = BuildTcpInfo(tcp);
                } else if (ip.PayloadPacket is UdpPacket udp) {
                    model.Protocol = "Udp";
                    model.Info = BuildUdpInfo(udp);
                } else if (ip.PayloadPacket is IcmpV4Packet icmp) {
                    model.Protocol = "Icmp";
                    model.Info = $"Type={icmp.TypeCode}";
                } else {
                    model.Protocol = ip.Protocol.ToString();
                    model.Info = $"IP {ip.SourceAddress} → {ip.DestinationAddress}";
                }
            } else if (eth.PayloadPacket is ArpPacket arp) {
                model.Protocol = "Arp";
                model.Info = BuildArpInfo(arp);
            } else {
                model.Info = $"Ethernet Type={eth.Type}";
            }
        } else {
            model.Protocol = "Unknown";
            model.Info = parsed?.ToString() ?? "N/A";
        }

        packets.Add(model);
        PacketCaptured?.Invoke(model);
    }

    private static string BuildTcpInfo (TcpPacket tcp) {
        var flags = new List<string>();
        if (tcp.Synchronize){
            flags.Add("SYN");
        }
        if (tcp.Acknowledgment){
            flags.Add("ACK");
        }
        if (tcp.Finished){
            flags.Add("FIN");
        }
        if (tcp.Reset){
            flags.Add("RST");
        }
        if (tcp.Push){
            flags.Add("PSH");
        }

        var flagStr = flags.Count > 0 ? string.Join(", ", flags) : "—";
        return $"{tcp.SourcePort} → " +
            $"{tcp.DestinationPort} [{flagStr}]" +
            $" Seq={tcp.SequenceNumber} Win={tcp.WindowSize}";
    }

    private static string BuildUdpInfo (UdpPacket udp) =>
        $"{udp.SourcePort} → {udp.DestinationPort} Len={udp.Length}";

    private static string BuildArpInfo (ArpPacket arp) =>
        arp.Operation == ArpOperation.Request
            ? $"Who has {arp.TargetProtocolAddress}? Tell {arp.SenderProtocolAddress}"
            : $"{arp.SenderProtocolAddress} is at {arp.SenderHardwareAddress}";
}
