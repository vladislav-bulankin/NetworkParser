using NetworkParser.ViewModels;
using NetworkParser.Views.Dialogs;

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
            viewModel.InterfaceSelectionRequested += async () => await ShowInterfaceDialogAsync(viewModel);
        }
    }
    private async Task ShowInterfaceDialogAsync (MainViewModel vm) {
        var dialog = new InterfaceSelectionDialog(vm);
        dialog.XamlRoot = this.XamlRoot;

        var result = await dialog.ShowAsync();

        if (result == ContentDialogResult.Primary) {
            vm.StartSniffing();
        }
    }
}
