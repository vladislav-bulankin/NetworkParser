using NetworkParser.Core.Abstractions.Parsers;
using NetworkParser.Domain.Packets;
using PacketDotNet;

namespace NetworkParser.Core.Parsers;

public class PacketParser : IPacketParser {
    private readonly ITlsParser tlsParser;
    public PacketParser (ITlsParser tlsParser) {
        this.tlsParser = tlsParser;
    }

    public void ParseEthernetPacket (IPPacket ip, ref PacketModel model)
        => _ = ip.PayloadPacket switch {
            TcpPacket tcp => ParseTcp(tcp, ref model),
            UdpPacket udp => ParseUdp(udp, ref model),
            IcmpV4Packet icmp => ParseIcmp(icmp, ref model),
            _ => DefauptParse(ip, ref model),
        };

    public void ParseArp (ArpPacket arp, ref PacketModel model) {
        model.Protocol = "Arp";
        model.Info = BuildArpInfo(arp);
    }

    private object ParseTcp (TcpPacket tcp, ref PacketModel model) {
        model.SourcePort = tcp.SourcePort;
        model.DestinationPort = tcp.DestinationPort;
        if ((tcp.DestinationPort == 443 || tcp.SourcePort == 443) &&
                tcp.PayloadData?.Length > 0) {
            var sni = tlsParser.ExtractSni(tcp.PayloadData);
            if (sni != null) {
                model.Protocol = "TLS";
                model.Info = $"ClientHello SNI: {sni}";
            } else if (tcp.PayloadData[0] == 0x17) {
                model.Protocol = "TLS";
                model.Info = $"{tcp.SourcePort} → " +
                    $"{tcp.DestinationPort} [Application Data] len={tcp.PayloadData.Length}";
            } else if (tcp.PayloadData[0] == 0x15) {
                model.Protocol = "TLS";
                model.Info = $"{tcp.SourcePort} → {tcp.DestinationPort} [Alert]";
            } else {
                model.Info = BuildTcpInfo(tcp);
            }
        } else {
            model.Protocol = "Tcp";
            model.Info = BuildTcpInfo(tcp);
        }
        return new();
    }

    private object ParseUdp (UdpPacket udp, ref PacketModel model) {
        model.Protocol = "Udp";
        model.Info = BuildUdpInfo(udp);
        model.SourcePort = udp.SourcePort;
        model.DestinationPort = udp.DestinationPort;
        return new();
    }

    private static string BuildTcpInfo (TcpPacket tcp) {
        var flags = new List<string>();
        var map = new (bool Condition, string Label)[]
        {
            (tcp.Synchronize, "SYN"),
            (tcp.Acknowledgment, "ACK"),
            (tcp.Finished, "FIN"),
            (tcp.Reset, "RST"),
            (tcp.Push, "PSH")
        };
        foreach (var (cond, label) in map) {
            if (cond) {
                flags.Add(label);
            }
        }
        var flagStr = flags.Count > 0 ? string.Join(", ", flags) : "—";
        return $"{tcp.SourcePort} → " +
            $"{tcp.DestinationPort} [{flagStr}]" +
            $" Seq={tcp.SequenceNumber} Win={tcp.WindowSize}";
    }

    private static string BuildUdpInfo (UdpPacket udp) =>
        $"{udp.SourcePort} → {udp.DestinationPort} Len={udp.Length}";

    private object ParseIcmp (IcmpV4Packet icmp, ref PacketModel model) {
        model.Protocol = "Icmp";
        model.Info = $"Type={icmp.TypeCode}";
        return new();
    }

    private object DefauptParse (IPPacket ip, ref PacketModel model) {
        model.Protocol = ip.Protocol.ToString();
        model.Info = $"IP {ip.SourceAddress} → {ip.DestinationAddress}";
        return new();
    }

    private string? BuildArpInfo (ArpPacket arp) =>
        arp.Operation == ArpOperation.Request
            ? $"Who has {arp.TargetProtocolAddress}? Tell {arp.SenderProtocolAddress}"
            : $"{arp.SenderProtocolAddress} is at {arp.SenderHardwareAddress}";
}
