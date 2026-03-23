using NetworkParser.Domain.Packets;
using PacketDotNet;

namespace NetworkParser.Core.Abstractions.Parsers;

public interface IPacketParser {
    void ParseEthernetPacket (IPPacket ip, ref PacketModel model);
    void ParseArp (ArpPacket arp, ref PacketModel model);
}
