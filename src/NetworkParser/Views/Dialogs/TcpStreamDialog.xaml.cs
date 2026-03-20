using NetworkParser.UI.ViewModels;

namespace NetworkParser.UI.Views.Dialogs;

public sealed partial class TcpStreamDialog : ContentDialog {
    public TcpStreamDialog (TcpStreamViewModel viewModel) {
        this.InitializeComponent();
        DataContext = viewModel;
    }
}
