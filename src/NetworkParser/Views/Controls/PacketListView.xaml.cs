// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

using Microsoft.UI.Dispatching;
using NetworkParser.UI.Views;
using NetworkParser.ViewModels;

namespace NetworkParser.Views.Controls;

public sealed partial class PacketListView : UserControl {
    public PacketListView () {
        this.InitializeComponent();
        this.Loaded += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine($"DC: {DataContext?.GetType().FullName}");
        };
    }
    
}
