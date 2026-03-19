using NetworkParser.ViewModels;

namespace NetworkParser.Views.Dialogs;

public sealed partial class InterfaceSelectionDialog : ContentDialog {
    private readonly MainViewModel viewModel;

    public InterfaceSelectionDialog (MainViewModel viewModel) {
        this.InitializeComponent();
        this.viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));

        DataContext = this.viewModel;  
    }

    private void ContentDialog_PrimaryButtonClick (ContentDialog sender, ContentDialogButtonClickEventArgs args) {
        viewModel.StartSniffing(); 
    }

    private void ContentDialog_SecondaryButtonClick (ContentDialog sender, ContentDialogButtonClickEventArgs args) {
        // Cancel — ничего не делаем
    }
}
