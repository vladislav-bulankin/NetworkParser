// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

using NetworkParser.ViewModels;
using Windows.ApplicationModel.DataTransfer;

namespace NetworkParser.Views.Controls;

public sealed partial class HexViewerView : UserControl {
    public HexViewerView () {
        this.InitializeComponent();
    }
    private void OnCopyHex (object sender, RoutedEventArgs e) {
        if (DataContext is MainViewModel mainVM) {
            var hex = mainVM.HexViewerVM.HexDump;
            if (!string.IsNullOrEmpty(hex)) {
                var dp = new DataPackage();
                dp.SetText(hex);
                Clipboard.SetContent(dp);
            }
        }
    }
}
