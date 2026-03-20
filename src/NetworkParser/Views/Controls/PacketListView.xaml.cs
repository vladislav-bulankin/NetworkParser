// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

using CommunityToolkit.WinUI.UI.Controls;
using Microsoft.UI;
using Microsoft.UI.Xaml.Input;
using NetworkParser.Domain.Packets;
using NetworkParser.UI.Views.Converters;
using NetworkParser.ViewModels;
using Windows.ApplicationModel.DataTransfer;

namespace NetworkParser.Views.Controls;

public sealed partial class PacketListView : UserControl {
    public PacketListView () {
        this.InitializeComponent();
    }

    private void OnRowDoubleTapped (object sender, DoubleTappedRoutedEventArgs e) {
        var grid = (Grid)this.Content;
        if (grid.DataContext is PacketListViewModel listVM && listVM.SelectedPacket != null) {
            listVM.TriggerPacketSelected();
        }
        if (DataContext is MainViewModel mainVM && mainVM.PacketListVM.SelectedPacket?.Protocol == "Tcp") {
            mainVM.ShowTcpStream(mainVM.PacketListVM.SelectedPacket, this.XamlRoot);
        }
    }
    private void OnCopyRow (object sender, RoutedEventArgs e) {
        var grid = (Grid)this.Content;
        if (grid.DataContext is PacketListViewModel vm && vm.SelectedPacket != null) {
            var p = vm.SelectedPacket;
            var text = $"{p.Number}\t{p.Timestamp:HH:mm:ss.fff}\t{p.Source}\t{p.Destination}\t{p.Protocol}\t{p.Length}\t{p.Info}";
            var dp = new DataPackage();
            dp.SetText(text);
            Clipboard.SetContent(dp);
        }
    }

    private void OnFollowStream (object sender, RoutedEventArgs e) {
        OnRowDoubleTapped(sender, null);
    }
    private void OnLoadingRow (object sender, DataGridRowEventArgs e) {
        if (e.Row.DataContext is PacketModel packet) {
            var converter = new ProtocolColorConverter();
            var brush = (SolidColorBrush)converter.Convert(
                packet.Protocol, typeof(SolidColorBrush), null, null);

            e.Row.Background = brush;
            e.Row.Foreground = new SolidColorBrush(Colors.White);
        }
    }

}
