using System;
using System.Collections.Generic;
using System.Text;
using NetworkParser.Domain.Interfaces;
using NetworkParser.Domain.Packets;

namespace NetworkParser.Core.Abstractions.Connection; 
public interface INetworkParserController {
    void StopCapture ();
    void ApplyFilter (string filter);
    public event Action<PacketModel> PacketCaptured;
    List<NetworkInterfaceModel> GetAvailableInterfaces ();
    void StartCapture (int deviceIndex = 0, string? bpfFilter = null);
    void SendPacket (byte[] rawData);
}
