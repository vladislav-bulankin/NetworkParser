// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

using Microsoft.UI.Xaml.Input;
using NetworkParser.ViewModels;

namespace NetworkParser.Views.Controls;

public sealed partial class PacketListView : UserControl {
    public PacketListView () {
        this.InitializeComponent();
    }

    private void OnRowDoubleTapped (object sender, DoubleTappedRoutedEventArgs e) {
        if (DataContext is MainViewModel mainVM) {
            mainVM.PacketListVM.TriggerPacketSelected();
        }
    }

}
