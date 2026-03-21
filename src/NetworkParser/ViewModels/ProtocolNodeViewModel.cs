using System.Collections.ObjectModel;
using System.ComponentModel;

namespace NetworkParser.UI.ViewModels;

public class ProtocolNodeViewModel : INotifyPropertyChanged {
    private string displayText;
    public string DisplayText
    {
        get => displayText;
        set {
            displayText = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DisplayText)));
        }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    public ObservableCollection<ProtocolNodeViewModel> Children { get; } = new();

    public ProtocolNodeViewModel (string text) {
        displayText = text;
    }
}
