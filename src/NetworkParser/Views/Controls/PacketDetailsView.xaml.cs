using System.Collections.ObjectModel;
using System.Collections.Specialized;
using NetworkParser.Domain.Protocols;
using NetworkParser.ViewModels;

namespace NetworkParser.Views.Controls;

public sealed partial class PacketDetailsView : UserControl {
    private MainViewModel? currentVM;
    public PacketDetailsView () {
        this.InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }
    private void OnDataContextChanged (FrameworkElement sender, DataContextChangedEventArgs args) {
        if (currentVM != null){
            currentVM.PacketDetailsVM.ProtocolTree.CollectionChanged -= OnTreeChanged;
        }

        if (DataContext is MainViewModel mainVM) {
            currentVM = mainVM;
            currentVM.PacketDetailsVM.ProtocolTree.CollectionChanged += OnTreeChanged;
            RebuildTree(currentVM.PacketDetailsVM.ProtocolTree);
        }
    }

    private void OnTreeChanged (object? sender, NotifyCollectionChangedEventArgs e) {
        if (currentVM != null)
            RebuildTree(currentVM.PacketDetailsVM.ProtocolTree);
    }

    private void RebuildTree (ObservableCollection<ProtocolModel> items) {
        ProtocolTreeView.RootNodes.Clear();

        foreach (var item in items) {
            var node = BuildNode(item);
            ProtocolTreeView.RootNodes.Add(node);
        }
    }

    private TreeViewNode BuildNode (ProtocolModel model) {
        var node = new TreeViewNode { Content = model.DisplayText, IsExpanded = true };
        foreach (var child in model.Children)
            node.Children.Add(BuildNode(child)); // рекурсия для любой глубины
        return node;
    }
}
