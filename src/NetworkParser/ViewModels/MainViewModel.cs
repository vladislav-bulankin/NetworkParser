using System.ComponentModel;
using NetworkParser.Core.Abstractions.Connection;
using NetworkParser.Core.Connection;
using NetworkParser.Domain.Packets;

namespace NetworkParser.ViewModels;

public class MainViewModel {
    private readonly INetworkParserController controller;
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

    public RelayCommand StartCaptureCommand { get; }
    public RelayCommand StopCaptureCommand { get; }
    public RelayCommand ApplyFilterCommand { get; }

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

        StartCaptureCommand = new RelayCommand(() => controller.StartCapture());
        StopCaptureCommand = new RelayCommand(() => controller.StopCapture());
        ApplyFilterCommand = new RelayCommand(() => controller.ApplyFilter(FilterText));
    }

    private void OnPacketSelected (PacketModel packet) {
        PacketDetailsVM.SetPacket(packet);
        HexViewerVM.SetPacket(packet);
    }
    private void OnPropertyChanged ([System.Runtime.CompilerServices.CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
