using NetworkParser.UI.ViewModels;

namespace NetworkParser.UI.Views.Dialogs;

public sealed partial class StatisticsDialog : ContentDialog {
    public StatisticsDialog (StatisticsViewModel viewModel) {
        this.InitializeComponent();
        DataContext = viewModel;
    }
}
