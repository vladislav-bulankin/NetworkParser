using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.UI.Dispatching;
using NetworkParser.Core.Abstractions.Connection;
using NetworkParser.Core.Helpers;
using NetworkParser.Domain.Interfaces;
using NetworkParser.Domain.Packets;
using NetworkParser.UI.ViewModels;
using NetworkParser.UI.Views.Dialogs;

namespace NetworkParser.ViewModels;

public class MainViewModel : INotifyPropertyChanged {
    private readonly INetworkParserController controller;
    public ObservableCollection<NetworkInterfaceModel> NetworkInterfaces { get; } = new();
    public event PropertyChangedEventHandler? PropertyChanged;
    public PacketListViewModel PacketListVM { get; }
    public PacketDetailsViewModel PacketDetailsVM { get; }
    public HexViewerViewModel HexViewerVM { get; }
    
    private string filterText;
    private Func<PacketModel, bool> currentFilter = _ => true;
    public string FilterText
    {
        get => filterText;
        set {
            filterText = value;
            OnPropertyChanged();

            currentFilter = PacketFilterBuilder.Build(filterText);
            PacketListVM.SetFilter(currentFilter);
        }
    }
    private NetworkInterfaceModel? selectedInterface;
    public NetworkInterfaceModel? SelectedInterface
    {
        get => selectedInterface;
        set {
            if (selectedInterface == value){
                return;
            }
            selectedInterface = value;
            OnPropertyChanged();
            if (selectedInterface != null) {
                OnPropertyChanged(nameof(CurrentInterfaceDisplay));
                StartSniffing();
            }
        }
    }
    public string CurrentInterfaceDisplay=>
            SelectedInterface == null
                    ? "No interface selected"
                    : $"Listening on: {SelectedInterface.FriendlyName}";
    
    public string CaptureFilter { get; set; }
    public RelayCommand StartCaptureCommand { get; }
    public RelayCommand StopCaptureCommand { get; }
    public RelayCommand ApplyFilterCommand { get; }
    public RelayCommand OpenInterfacesCommand { get; }
    public RelayCommand SaveCaptureCommand { get; }
    public RelayCommand OpenCaptureCommand { get; }
    public RelayCommand ShowStatisticsCommand { get; }
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
        ApplyFilterCommand = new RelayCommand(() =>
        {
            controller.ApplyFilter(CaptureFilter);
            PacketListVM.SetFilter(PacketFilterBuilder.Build(FilterText));
        });
        SaveCaptureCommand = new RelayCommand(() => SaveCaptureRequested?.Invoke());
        OpenCaptureCommand = new RelayCommand(() => OpenCaptureRequested?.Invoke());
        ShowStatisticsCommand = new RelayCommand(() => StatisticsRequested?.Invoke());
    }
    public event Action? StatisticsRequested;
    public event Action? SaveCaptureRequested;
    public event Action? OpenCaptureRequested;
    public void SaveCapture (string filePath) {
        controller.SaveCapture(filePath);
    }

    public void LoadCapture (string filePath) {
        controller.StopCapture();
        PacketListVM.Clear();
        var loaded = controller.LoadCapture(filePath);
        foreach (var p in loaded){
            PacketListVM.AddPacket(p);
        }
    }
    public void StartSniffing () {
        if (SelectedInterface == null){ return; }
        PacketListVM.Clear();
        controller.StopCapture();
        controller.StartCapture(SelectedInterface.Index, CaptureFilter);
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
    public async void ShowTcpStream (PacketModel packet, XamlRoot xamlRoot) {
        if (packet.Protocol != "Tcp"){ return; }
        var vm = new TcpStreamViewModel();
        vm.BuildStream(packet, PacketListVM.AllPackets);

        var dialog = new TcpStreamDialog(vm);
        dialog.XamlRoot = xamlRoot;
        await dialog.ShowAsync();
    }
    private void OnPacketSelected (PacketModel packet) {
        PacketDetailsVM.SetPacket(packet);
        HexViewerVM.SetPacket(packet);
    }
    private void OnPropertyChanged ([System.Runtime.CompilerServices.CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
