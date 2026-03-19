using System;
using System.Collections.Generic;
using System.Text;
using NetworkParser.Domain.Packets;

namespace NetworkParser.Core.Abstractions.Connection; 
public interface INetworkParserController {
    void StartCapture ();
    void StopCapture ();
    void ApplyFilter (string filter);
    public event Action<PacketModel> PacketCaptured;
}
