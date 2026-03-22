using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using NetworkParser.Domain.Packets;
using NetworkParser.Domain.Protocols;
using PacketDotNet;

namespace NetworkParser.ViewModels;

public class PacketDetailsViewModel : INotifyPropertyChanged {
    public ObservableCollection<ProtocolModel> ProtocolTree { get; }
        = new ObservableCollection<ProtocolModel>();
    public event PropertyChangedEventHandler? PropertyChanged;
    private ConcurrentDictionary<string, string> dnsChach = new();
    public async Task SetPacket (PacketModel packet) {
        ProtocolTree.Clear();
        if (packet == null) { return; }
        var parsed = Packet.ParsePacket(LinkLayers.Ethernet, packet.RawData);

        if (parsed is EthernetPacket eth) {
            var ethNode = new ProtocolModel("Ethernet");
            ethNode.Children.Add(new ProtocolModel($"Source MAC: {eth.SourceHardwareAddress}"));
            ethNode.Children.Add(new ProtocolModel($"Destination MAC: {eth.DestinationHardwareAddress}"));
            ethNode.Children.Add(new ProtocolModel($"Type: {eth.Type}"));
            ProtocolTree.Add(ethNode);
            if (eth.PayloadPacket is IPPacket ip) {
                if (!dnsChach.Keys.Contains(ip.SourceAddress.ToString())) {
                    await AddToDnsCachAsync(ip.SourceAddress.ToString());
                }
                if (!dnsChach.Keys.Contains(ip.DestinationAddress.ToString())) {
                    await AddToDnsCachAsync(ip.DestinationAddress.ToString());
                }
                    var srcIpStr = $"{ip.SourceAddress.ToString()} | " +
                        $"{dnsChach[ip.SourceAddress.ToString()]}";
                    var dstIpStr = $"{ip.DestinationAddress.ToString()} | " +
                        $"{dnsChach[ip.DestinationAddress.ToString()]}";
                    var ipNode = new ProtocolModel($"Internet Protocol ({ip.Version})");
                ipNode.Children.Add(new ProtocolModel($"Source: {srcIpStr}"));
                ipNode.Children.Add(new ProtocolModel($"Destination: {dstIpStr}"));
                ipNode.Children.Add(new ProtocolModel($"TTL: {ip.TimeToLive}"));
                ipNode.Children.Add(new ProtocolModel($"Protocol: {ip.Protocol}"));
                var headerLen = ip.Version == IPVersion.IPv6 ? 40 : ip.HeaderLength;
                ipNode.Children.Add(new ProtocolModel($"Header Length: {headerLen} bytes"));
                ProtocolTree.Add(ipNode);
                if (ip.PayloadPacket is TcpPacket tcp) {
                    var tcpNode = new ProtocolModel("Transmission Control Protocol");
                    tcpNode.Children.Add(new ProtocolModel($"Source Port: {tcp.SourcePort}"));
                    tcpNode.Children.Add(new ProtocolModel($"Destination Port: {tcp.DestinationPort}"));
                    tcpNode.Children.Add(new ProtocolModel($"Sequence Number: {tcp.SequenceNumber}"));
                    tcpNode.Children.Add(new ProtocolModel($"Acknowledgment: {tcp.AcknowledgmentNumber}"));
                    tcpNode.Children.Add(new ProtocolModel($"Window Size: {tcp.WindowSize}"));
                    var flagsNode = new ProtocolModel("Flags");
                    flagsNode.Children.Add(new ProtocolModel($"SYN: {tcp.Synchronize}"));
                    flagsNode.Children.Add(new ProtocolModel($"ACK: {tcp.Acknowledgment}"));
                    flagsNode.Children.Add(new ProtocolModel($"FIN: {tcp.Finished}"));
                    flagsNode.Children.Add(new ProtocolModel($"RST: {tcp.Reset}"));
                    flagsNode.Children.Add(new ProtocolModel($"PSH: {tcp.Push}"));
                    tcpNode.Children.Add(flagsNode);
                    ProtocolTree.Add(tcpNode);
                } else if (ip.PayloadPacket is UdpPacket udp) {
                    var udpNode = new ProtocolModel("User Datagram Protocol");
                    udpNode.Children.Add(new ProtocolModel($"Source Port: {udp.SourcePort}"));
                    udpNode.Children.Add(new ProtocolModel($"Destination Port: {udp.DestinationPort}"));
                    udpNode.Children.Add(new ProtocolModel($"Length: {udp.Length}"));
                    udpNode.Children.Add(new ProtocolModel($"Checksum: 0x{udp.Checksum:X4}"));
                    ProtocolTree.Add(udpNode);
                } else if (ip.PayloadPacket is IcmpV4Packet icmp) {
                    var icmpNode = new ProtocolModel("Internet Control Message Protocol");
                    icmpNode.Children.Add(new ProtocolModel($"Type: {icmp.TypeCode}"));
                    icmpNode.Children.Add(new ProtocolModel($"Checksum: 0x{icmp.Checksum:X4}"));
                    ProtocolTree.Add(icmpNode);
                }
            } else if (eth.PayloadPacket is ArpPacket arp) {
                var arpNode = new ProtocolModel("Address Resolution Protocol");
                arpNode.Children.Add(new ProtocolModel($"Operation: {arp.Operation}"));
                arpNode.Children.Add(new ProtocolModel($"Sender MAC: {arp.SenderHardwareAddress}"));
                arpNode.Children.Add(new ProtocolModel($"Sender IP: {arp.SenderProtocolAddress}"));
                arpNode.Children.Add(new ProtocolModel($"Target MAC: {arp.TargetHardwareAddress}"));
                arpNode.Children.Add(new ProtocolModel($"Target IP: {arp.TargetProtocolAddress}"));
                ProtocolTree.Add(arpNode);
            }
        }
    }

    protected virtual void OnPropertyChanged
        ([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private async Task AddToDnsCachAsync(string ip) {
        IPHostEntry entry = null;
        try {
            entry = await Dns
                .GetHostEntryAsync(ip)
                .WaitAsync(TimeSpan.FromSeconds(2));
        } catch { /*ignore*/ } finally {
            dnsChach.AddOrUpdate(
                ip,
                entry?.HostName ?? string.Empty,  
                (key, oldValue) => entry?.HostName ?? string.Empty 
            );
        }
    }
}
