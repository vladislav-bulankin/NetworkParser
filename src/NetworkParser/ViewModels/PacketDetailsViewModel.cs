using System.Collections.ObjectModel;
using System.ComponentModel;
using NetworkParser.Domain.Packets;
using NetworkParser.Domain.Protocols;

namespace NetworkParser.ViewModels;

public class PacketDetailsViewModel : INotifyPropertyChanged {
    public ObservableCollection<ProtocolModel> ProtocolTree { get; }
        = new ObservableCollection<ProtocolModel>();
    public event PropertyChangedEventHandler? PropertyChanged;
    public void SetPacket (PacketModel packet) {
        ProtocolTree.Clear();

        if (packet == null){
            return;
        }

        // Ethernet
        var eth = new ProtocolModel("Ethernet");
        eth.Children.Add(new ProtocolModel($"Length: {packet.Length}"));

        // IP
        var ip = new ProtocolModel("IP");
        ip.Children.Add(new ProtocolModel($"Source: {packet.Source}"));
        ip.Children.Add(new ProtocolModel($"Destination: {packet.Destination}"));

        // Protocol
        var proto = new ProtocolModel(packet.Protocol);

        ProtocolTree.Add(eth);
        ProtocolTree.Add(ip);
        ProtocolTree.Add(proto);
    }

    protected virtual void OnPropertyChanged
        ([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
