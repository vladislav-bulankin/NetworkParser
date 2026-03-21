using NetworkParser.UI.ViewModels;

namespace NetworkParser.UI.Views.Dialogs;

public sealed partial class PacketCrafterDialog : ContentDialog {
    public PacketCrafterDialog (PacketCrafterViewModel viewModel) {
        this.InitializeComponent();
        DataContext = viewModel;
    }
}
