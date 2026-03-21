using System.ComponentModel;
using System.Net;
using System.Net.NetworkInformation;
using NetworkParser.Core.Abstractions.Connection;
using PacketDotNet;

namespace NetworkParser.UI.ViewModels;

public class PacketCrafterViewModel : INotifyPropertyChanged {
    private readonly INetworkParserController controller;
    public event PropertyChangedEventHandler? PropertyChanged;

    // --- ARP Spoofing ---
    public string ArpTargetIp { get; set; } = "";
    public string ArpSpoofedIp { get; set; } = "";
    public string ArpSenderMac { get; set; } = "";

    // --- TCP RST ---
    public string RstSrcIp { get; set; } = "";
    public string RstDstIp { get; set; } = "";
    public string RstSrcPort { get; set; } = "";
    public string RstDstPort { get; set; } = "";
    public string ArpStatus { get => arpStatus; private set { arpStatus = value; OnPropertyChanged(); } }
    public string RstStatus { get => rstStatus; private set { rstStatus = value; OnPropertyChanged(); } }
    public string HexStatus { get => hexStatus; private set { hexStatus = value; OnPropertyChanged(); } }
    private string hexInput = "";
    public string HexInput
    {
        get => hexInput;
        set { hexInput = value; OnPropertyChanged(); }
    }

    private string arpStatus = "";
    private string rstStatus = "";
    private string hexStatus = "";

    public RelayCommand SendArpCommand { get; }
    public RelayCommand SendRstCommand { get; }
    public RelayCommand SendHexCommand { get; }

    public PacketCrafterViewModel (INetworkParserController controller) {
        this.controller = controller;
        SendArpCommand = new RelayCommand(SendArp);
        SendRstCommand = new RelayCommand(SendRst);
        SendHexCommand = new RelayCommand(SendHex);
    }

    private void SendArp () {
        try {
            if (!IPAddress.TryParse(ArpTargetIp, out var targetIp) ||
                !IPAddress.TryParse(ArpSpoofedIp, out var spoofedIp)) {
                ArpStatus = "Invalid IP address";
                return;
            }

            var senderMac = string.IsNullOrWhiteSpace(ArpSenderMac)
                ? PhysicalAddress.Parse("DE:AD:BE:EF:00:01")
                : PhysicalAddress.Parse(ArpSenderMac.Replace(":", "-").ToUpper());

            var arp = new ArpPacket(
                ArpOperation.Response,
                PhysicalAddress.Parse("FF-FF-FF-FF-FF-FF"),
                targetIp,
                senderMac,
                spoofedIp);

            var eth = new EthernetPacket(
                senderMac,
                PhysicalAddress.Parse("FF-FF-FF-FF-FF-FF"),
                EthernetType.Arp);

            eth.PayloadPacket = arp;
            controller.SendPacket(eth.Bytes);
            ArpStatus = $"Sent ARP Reply: {spoofedIp} is at {senderMac}";
        } catch (Exception ex) {
            ArpStatus = $"Error: {ex.Message}";
        }
    }

    private void SendRst () {
        try {
            if (!IPAddress.TryParse(RstSrcIp, out var srcIp) ||
                !IPAddress.TryParse(RstDstIp, out var dstIp) ||
                !int.TryParse(RstSrcPort, out var srcPort) ||
                !int.TryParse(RstDstPort, out var dstPort)) {
                RstStatus = "Invalid input";
                return;
            }

            var tcp = new TcpPacket((ushort)srcPort, (ushort)dstPort) {
                Reset = true,
                SequenceNumber = 0
            };

            var ip = new IPv4Packet(srcIp, dstIp) {
                Protocol = ProtocolType.Tcp,
                PayloadPacket = tcp
            };

            var eth = new EthernetPacket(
                PhysicalAddress.Parse("DE-AD-BE-EF-00-01"),
                PhysicalAddress.Parse("FF-FF-FF-FF-FF-FF"),
                EthernetType.IPv4) {
                PayloadPacket = ip
            };

            tcp.UpdateTcpChecksum();
            ip.UpdateIPChecksum();

            controller.SendPacket(eth.Bytes);
            RstStatus = $"Sent TCP RST: {srcIp}:{srcPort} → {dstIp}:{dstPort}";
        } catch (Exception ex) {
            RstStatus = $"Error: {ex.Message}";
        }
    }

    private void SendHex () {
        try {
            var hex = HexInput.Replace(" ", "").Replace("\n", "").Replace("\r", "");
            if (hex.Length % 2 != 0) {
                HexStatus = "Invalid hex — odd number of chars";
                return;
            }
            var bytes = Convert.FromHexString(hex);
            controller.SendPacket(bytes);
            HexStatus = $"Sent {bytes.Length} bytes";
        } catch (Exception ex) {
           HexStatus = $"Error: {ex.Message}";
        }
    }

    protected void OnPropertyChanged ([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
