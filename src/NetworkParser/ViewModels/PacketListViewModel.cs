using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.UI.Dispatching;
using NetworkParser.Core.Abstractions.Connection;
using NetworkParser.Domain.Packets;

namespace NetworkParser.ViewModels;

public class PacketListViewModel : INotifyPropertyChanged {
    private readonly INetworkParserController controller;
    private DispatcherQueue dispatcher;
    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<PacketModel> Packets { get; } = new();

    private PacketModel? selectedPacket;
    public PacketModel? SelectedPacket
    {
        get => selectedPacket;
        set {
            selectedPacket = value;
            OnPropertyChanged();
            PacketSelected?.Invoke(value!);
        }
    }

    public event Action<PacketModel>? PacketSelected;

    public PacketListViewModel (INetworkParserController controller) {
        this.controller = controller ?? throw new ArgumentNullException(nameof(controller));
        
        this.controller.PacketCaptured += OnPacketCaptured;
    }

    private void OnPacketCaptured (PacketModel packet) {
        dispatcher.TryEnqueue(() =>
        {
            packet.Number = Packets.Count + 1;
            Packets.Add(packet);
        });
    }

    protected virtual void OnPropertyChanged 
            ([System.Runtime.CompilerServices.CallerMemberName] string? propertyName = null) {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public void Dispose () {
        controller.PacketCaptured -= OnPacketCaptured;
    }

    internal void SetDispatcher (DispatcherQueue dispatcher) {
        this.dispatcher = dispatcher;
    }
}
