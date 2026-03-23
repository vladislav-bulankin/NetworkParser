using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using NetworkParser.Domain.Packets;
using NetworkParser.Domain.Protocols;
using NetworkParser.UI.Mappers;
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
            var ethNode = ProtocolMapper.MapEthernetPacket(eth);
            ProtocolTree.Add(ethNode);
            if (eth.PayloadPacket is IPPacket ip) {
                if (!dnsChach.Keys.Contains(ip.SourceAddress.ToString())) {
                    await AddToDnsCachAsync(ip.SourceAddress.ToString());
                }
                if (!dnsChach.Keys.Contains(ip.DestinationAddress.ToString())) {
                    await AddToDnsCachAsync(ip.DestinationAddress.ToString());
                }
                var dnsSrcName = dnsChach.GetValueOrDefault(ip.SourceAddress.ToString()) ?? string.Empty;
                var dnsDistName = dnsChach.GetValueOrDefault(ip.DestinationAddress.ToString()) ?? string.Empty;
                var ipNode = ProtocolMapper
                    .MapIPPacket(ip, dnsSrcName, dnsDistName);
                ProtocolTree.Add(ipNode);
                if (ip.PayloadPacket is TcpPacket tcp) {
                    var tcpNode = ProtocolMapper.MapTcpPacket(tcp);
                    ProtocolTree.Add(tcpNode);
                } else if (ip.PayloadPacket is UdpPacket udp) {
                    var udpNode = ProtocolMapper.MapUdpPacket(udp);
                    ProtocolTree.Add(udpNode);
                } else if (ip.PayloadPacket is IcmpV4Packet icmp) {
                    var icmpNode = ProtocolMapper.MapIcmpV4Packet(icmp);
                    ProtocolTree.Add(icmpNode);
                }
            } else if (eth.PayloadPacket is ArpPacket arp) {
                var arpNode = ProtocolMapper.MapArpPacket(arp);
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
