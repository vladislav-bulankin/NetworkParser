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
    public ObservableCollection<PacketModel> AllPackets { get; } = new();
    public ObservableCollection<PacketModel> FilteredPackets { get; } = new();
    const int MaxPackets = 5000;
    private PacketModel? selectedPacket;
    private Func<PacketModel, bool> currentFilter = _ => true;
    public int TotalCount => AllPackets.Count;
    public int DisplayedCount => FilteredPackets.Count;
    public string FilterStatus => TotalCount == DisplayedCount
        ? ""
        : $"| Displayed: {DisplayedCount}";
    public void SetFilter (Func<PacketModel, bool> filter) {
        currentFilter = filter ?? (_ => true);
        ApplyFilter();
    }
    public PacketModel? SelectedPacket
    {
        get => selectedPacket;
        set {
            selectedPacket = value;
            OnPropertyChanged();
        }
    }

    public void TriggerPacketSelected() {
        if (selectedPacket != null){
            PacketSelected?.Invoke(selectedPacket);
        }
    }

    public event Action<PacketModel>? PacketSelected;
    public void Clear () {
        FilteredPackets.Clear();
        AllPackets.Clear();
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(DisplayedCount));
        OnPropertyChanged(nameof(FilterStatus));
    }
    public PacketListViewModel (INetworkParserController controller) {
        this.controller = controller ?? throw new ArgumentNullException(nameof(controller));
        this.controller.PacketCaptured += OnPacketCaptured;
    }

    private void OnPacketCaptured (PacketModel packet) {
        dispatcher.TryEnqueue(() =>
        {
            if (AllPackets.Count > MaxPackets) {
                AllPackets.RemoveAt(0);
                FilteredPackets.RemoveAt(0);
            }
            packet.Number = AllPackets.Count + 1;
            AllPackets.Add(packet);
            if (currentFilter(packet)) {
                FilteredPackets.Add(packet);
                OnPropertyChanged(nameof(TotalCount));     
                OnPropertyChanged(nameof(DisplayedCount));
                OnPropertyChanged(nameof(FilterStatus));
            }
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
    public void ApplyFilter () {
        this.Clear();
        foreach (var packet in AllPackets) {
            if (currentFilter(packet)) {
                FilteredPackets.Add(packet);
            }
        }
        OnPropertyChanged(nameof(TotalCount));
        OnPropertyChanged(nameof(DisplayedCount));
        OnPropertyChanged(nameof(FilterStatus));
    }
}
