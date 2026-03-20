using NetworkParser.ViewModels;

namespace NetworkParser.Views.Dialogs;

public sealed partial class InterfaceSelectionDialog : ContentDialog {
    private readonly MainViewModel viewModel;
    private bool isLoaded = false;
    public InterfaceSelectionDialog (MainViewModel viewModel) {
        this.InitializeComponent();
        this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
        DataContext = this.viewModel;
        Loaded += (_, _) => isLoaded = true;
    }
    private void OnInterfaceSelected (object sender, SelectionChangedEventArgs e) {
        if (isLoaded && e.AddedItems.Count > 0) {
            Hide();
        }
    }
}
