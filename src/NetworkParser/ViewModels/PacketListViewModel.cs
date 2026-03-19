using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.UI.Dispatching;
using NetworkParser.Core.Abstractions.Connection;
using NetworkParser.Domain.Packets;

namespace NetworkParser.ViewModels;

public class PacketListViewModel {
    private readonly INetworkParserController controller;
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<PacketModel> Packets { get; } = new();

    private PacketModel selectedPacket;
    public PacketModel SelectedPacket
    {
        get => selectedPacket;
        set {
            selectedPacket = value;
            OnPropertyChanged();
            PacketSelected?.Invoke(value);
        }
    }

    public event Action<PacketModel> PacketSelected;

    public PacketListViewModel (INetworkParserController controller) {
        this.controller = controller;
        this.controller.PacketCaptured += AddPacket;
    }

    private void AddPacket (PacketModel packet) {
        DispatcherQueue.GetForCurrentThread().TryEnqueue(() => {
            packet.Number = Packets.Count + 1;
            Packets.Add(packet);
        });
    }
    private void OnPropertyChanged ([System.Runtime.CompilerServices.CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
