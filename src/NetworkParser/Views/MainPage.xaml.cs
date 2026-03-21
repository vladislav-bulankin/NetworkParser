using Microsoft.UI.Xaml.Input;
using NetworkParser.UI.ViewModels;
using NetworkParser.UI.Views.Dialogs;
using NetworkParser.ViewModels;
using NetworkParser.Views.Dialogs;
using Uno.Extensions.Specialized;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace NetworkParser.UI.Views;

/// <summary>
/// An empty page that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class MainPage : Page {
    public MainViewModel ViewModel { get; }
    private int searchIndex = 0;
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

    private void OnSearchTextChanged (object sender, TextChangedEventArgs e) {
        searchIndex = -1; 
        var vm = ViewModel.PacketListVM;

        if (string.IsNullOrWhiteSpace(SearchBox.Text)) {
            SearchResultText.Text = "";
            return;
        }

        var result = vm.SearchNext(SearchBox.Text, 0);
        if (result != null) {
            searchIndex = vm.FilteredPackets.IndexOf(result);
            vm.SelectedPacket = result;
            PacketListControl.ScrollToPacket(result);
            SearchResultText.Text = $"1 of {CountMatches(vm, SearchBox.Text)}";
        } else {
            SearchResultText.Text = "Not found";
        }
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
    private void OnCloseSearch (object sender, RoutedEventArgs e) {
        ViewModel.PacketListVM.IsSearchVisible = false;
        SearchBox.Text = "";
        SearchResultText.Text = "";
        searchIndex = 0;
    }

    private void OnSearchKeyDown (object sender, KeyRoutedEventArgs e) {
        if (e.Key == Windows.System.VirtualKey.Enter) {
            OnSearchNext(sender, e);
        }
        if (e.Key == Windows.System.VirtualKey.Escape) {
            OnCloseSearch(sender, e);
        }
    }

    private void OnSearchNext (object sender, RoutedEventArgs e) {
        var vm = ViewModel.PacketListVM; 

        if (string.IsNullOrWhiteSpace(SearchBox.Text)){ return; }
        var result = vm.SearchNext(SearchBox.Text, searchIndex + 1);
        if (result == null) {
            searchIndex = -1;
            result = vm.SearchNext(SearchBox.Text, 0);
        }

        if (result != null) {
            searchIndex = vm.FilteredPackets.IndexOf(result);
            vm.SelectedPacket = result;
            PacketListControl.ScrollToPacket(result);
            SearchResultText.Text = $"{searchIndex + 1} of {CountMatches(vm, SearchBox.Text)}";
        } else { SearchResultText.Text = "Not found"; }
    }

    private void OnSearchPrev (object sender, RoutedEventArgs e) {
        var vm = ViewModel.PacketListVM;

        if (string.IsNullOrWhiteSpace(SearchBox.Text)){ return; }
        var query = SearchBox.Text.ToLower();
        var matches = vm.FilteredPackets
            .Where(p =>
                (p.Source?.ToLower().Contains(query) ?? false) ||
                (p.Destination?.ToLower().Contains(query) ?? false) ||
                (p.Protocol?.ToLower().Contains(query) ?? false) ||
                (p.Info?.ToLower().Contains(query) ?? false) ||
                p.Number.ToString().Contains(query))
            .ToList();

        if (matches.Count == 0) { SearchResultText.Text = "Not found"; return; }
        var prevIndex = matches.FindLastIndex(p => vm.FilteredPackets.IndexOf(p) < searchIndex);
        var result = prevIndex >= 0 ? matches[prevIndex] : matches.Last();

        searchIndex = vm.FilteredPackets.IndexOf(result);
        vm.SelectedPacket = result;
        PacketListControl.ScrollToPacket(result);
        SearchResultText.Text = $"{searchIndex + 1} of {matches.Count}";
    }

    private int CountMatches (PacketListViewModel vm, string query) {
        query = query.ToLower();
        return vm.FilteredPackets.Count(p =>
            (p.Source?.ToLower().Contains(query) ?? false) ||
            (p.Destination?.ToLower().Contains(query) ?? false) ||
            (p.Protocol?.ToLower().Contains(query) ?? false) ||
            (p.Info?.ToLower().Contains(query) ?? false) ||
            p.Number.ToString().Contains(query));
    }
}
