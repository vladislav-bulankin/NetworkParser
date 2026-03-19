using NetworkParser.Core.Abstractions.Connection;
using NetworkParser.Domain.Packets;

namespace NetworkParser.Core.Connection; 
public class NetworkParserController : INetworkParserController {
    private readonly List<PacketModel> packets = new();

    public event Action<PacketModel> PacketCaptured;

    public void StartCapture () {
        // запуск sniffing
    }

    public void StopCapture () {
        // остановка
    }

    public void ApplyFilter (string filter) {
        // фильтрация пакетов
    }

    private void OnPacketCaptured (PacketModel packet) {
        packets.Add(packet);
        PacketCaptured?.Invoke(packet);
    }
}
