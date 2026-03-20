using NetworkParser.UI.ViewModels;
using NetworkParser.UI.Views.Dialogs;
using NetworkParser.ViewModels;
using NetworkParser.Views.Dialogs;
using Windows.Storage.Pickers;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace NetworkParser.UI.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page {
    public MainViewModel ViewModel { get; }
    public MainPage (MainViewModel viewModel) {
        this.InitializeComponent();
        ViewModel = viewModel;
        viewModel.InitializeDispatcher(this.DispatcherQueue);
        DataContext = viewModel;
        if (viewModel != null) {
            viewModel.InterfaceSelectionRequested += async () =>
                await ShowInterfaceDialogAsync(viewModel);
        }
        viewModel.SaveCaptureRequested += OnSaveCapture;
        viewModel.OpenCaptureRequested += OnOpenCapture;
        viewModel.StatisticsRequested += async () => {
            var dialog = new StatisticsDialog(new StatisticsViewModel());
            ((StatisticsViewModel)dialog.DataContext).Build(viewModel.PacketListVM.AllPackets);
            dialog.XamlRoot = this.XamlRoot;
            await dialog.ShowAsync();
        };
    }
    private async Task ShowInterfaceDialogAsync (MainViewModel vm) {
        var dialog = new InterfaceSelectionDialog(vm);
        dialog.XamlRoot = this.XamlRoot;
        await dialog.ShowAsync();
    }

    private void OnSaveCapture () {
        var path = Win32FileDialog.ShowSaveDialog(
            title: "Save Capture",
            filter: "Pcap files (*.pcap)\0*.pcap\0All files\0*.*\0",
            defaultExt: "pcap",
            defaultName: $"capture_{DateTime.Now:yyyyMMdd_HHmmss}");

        if (path != null){
            ((MainViewModel)DataContext).SaveCapture(path);
        }
    }

    private void OnOpenCapture () {
        var path = Win32FileDialog.ShowOpenDialog(
            title: "Open Capture",
            filter: "Pcap files (*.pcap *.pcapng)\0*.pcap;*.pcapng\0All files\0*.*\0");

        if (path != null){
            ((MainViewModel)DataContext).LoadCapture(path);
        }
    }
}
