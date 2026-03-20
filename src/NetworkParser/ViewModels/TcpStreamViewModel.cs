using System.ComponentModel;
using System.Text;
using NetworkParser.Domain.Packets;
using PacketDotNet;

namespace NetworkParser.UI.ViewModels;

public class TcpStreamViewModel : INotifyPropertyChanged {
    public event PropertyChangedEventHandler? PropertyChanged;
    public string StreamContent { get; private set; } = "";
    public string SessionKey { get; private set; } = "";

    public void BuildStream (PacketModel selected, IEnumerable<PacketModel> allPackets) {
        // нормализуем ключ сессии — всегда меньший IP первым
        var srcKey = $"{selected.Source}:{selected.SourcePort}";
        var dstKey = $"{selected.Destination}:{selected.DestinationPort}";

        var (keyA, keyB) = string.Compare(srcKey, dstKey) < 0
            ? (srcKey, dstKey)
            : (dstKey, srcKey);

        SessionKey = $"{keyA} ↔ {keyB}";

        // собираем все пакеты этой сессии
        var sessionPackets = allPackets
            .Where(p => p.Protocol == "Tcp" &&
                ((p.Source == selected.Source && p.SourcePort == selected.SourcePort &&
                  p.Destination == selected.Destination && p.DestinationPort == selected.DestinationPort) ||
                 (p.Source == selected.Destination && p.SourcePort == selected.DestinationPort &&
                  p.Destination == selected.Source && p.DestinationPort == selected.SourcePort)))
            .OrderBy(p => p.Timestamp)
            .ToList();

        var sb = new StringBuilder();
        foreach (var packet in sessionPackets) {
            var parsed = Packet.ParsePacket(LinkLayers.Ethernet, packet.RawData);
            if (parsed is EthernetPacket eth &&
                eth.PayloadPacket is IPPacket ip &&
                ip.PayloadPacket is TcpPacket tcp &&
                tcp.PayloadData?.Length > 0) {

                var direction = packet.Source == selected.Source ? "→" : "←";
                var text = TryDecodeAsText(tcp.PayloadData);
                sb.AppendLine($"[{packet.Timestamp:HH:mm:ss.fff}] {packet.Source}:{packet.SourcePort} {direction} {packet.Destination}:{packet.DestinationPort}");
                sb.AppendLine(text);
                sb.AppendLine();
            }
        }

        StreamContent = sb.Length > 0 ? sb.ToString() : "No payload data in this stream.";
    }

    private static string TryDecodeAsText (byte[] data) {
        // проверяем печатаемые символы
        var printable = data.Count(b => b >= 32 && b < 127 || b == '\n' || b == '\r' || b == '\t');
        if (printable / (double)data.Length > 0.7)
            return Encoding.UTF8.GetString(data);

        // иначе hex
        var sb = new StringBuilder();
        for (int i = 0; i < data.Length; i++) {
            if (i % 16 == 0 && i > 0)
                sb.AppendLine();
            sb.Append(data[i].ToString("X2") + " ");
        }
        return sb.ToString();
    }
}
