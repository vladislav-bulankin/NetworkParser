using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.UI.Dispatching;
using NetworkParser.Core.Abstractions.Connection;
using NetworkParser.Domain.Interfaces;
using NetworkParser.Domain.Packets;

namespace NetworkParser.ViewModels;

public class MainViewModel {
    private readonly INetworkParserController controller;
    public ObservableCollection<NetworkInterfaceModel> NetworkInterfaces { get; } = new();
    public event PropertyChangedEventHandler? PropertyChanged;
    public PacketListViewModel PacketListVM { get; }
    public PacketDetailsViewModel PacketDetailsVM { get; }
    public HexViewerViewModel HexViewerVM { get; }

    private string filterText;
    public string FilterText
    {
        get => filterText;
        set { filterText = value; OnPropertyChanged(); }
    }
    private NetworkInterfaceModel? selectedInterface;
    public NetworkInterfaceModel? SelectedInterface
    {
        get => selectedInterface;
        set {
            selectedInterface = value;
            OnPropertyChanged();
        }
    }
    public RelayCommand StartCaptureCommand { get; }
    public RelayCommand StopCaptureCommand { get; }
    public RelayCommand ApplyFilterCommand { get; }
    public RelayCommand OpenInterfacesCommand { get; }

    public MainViewModel (
            INetworkParserController controller,
            PacketListViewModel listVM,
            PacketDetailsViewModel detailsVM,
            HexViewerViewModel hexVM) {
        this.controller = controller;

        PacketListVM = listVM;
        PacketDetailsVM = detailsVM;
        HexViewerVM = hexVM;
        PacketListVM.PacketSelected += OnPacketSelected;
        OpenInterfacesCommand = new RelayCommand(() => GetInterfaces());
        StartCaptureCommand = new RelayCommand(() => StartSniffing());
        StopCaptureCommand = new RelayCommand(() => controller.StopCapture());
        ApplyFilterCommand = new RelayCommand(() => controller.ApplyFilter(FilterText));
    }
    public void StartSniffing () {
        controller.StartCapture(SelectedInterface?.Index ?? 4, FilterText);
    }
    public void InitializeDispatcher (DispatcherQueue dispatcher) {
        PacketListVM.SetDispatcher(dispatcher);
    }
    public void GetInterfaces () {
        var interfaces = controller.GetAvailableInterfaces();

        NetworkInterfaces.Clear();
        foreach (var iface in interfaces) {
            NetworkInterfaces.Add(iface);
        }
        SelectedInterface = NetworkInterfaces
            .FirstOrDefault(i => i.IsUp) ?? NetworkInterfaces.FirstOrDefault();
        InterfaceSelectionRequested?.Invoke();
    }
    public event Action? InterfaceSelectionRequested;
    private void OnPacketSelected (PacketModel packet) {
        PacketDetailsVM.SetPacket(packet);
        HexViewerVM.SetPacket(packet);
    }
    private void OnPropertyChanged ([System.Runtime.CompilerServices.CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
