using System.ComponentModel;
using System.Text;
using NetworkParser.Domain.Packets;

namespace NetworkParser.ViewModels;

public class HexViewerViewModel : INotifyPropertyChanged {
    private string hexDump;
    public string HexDump
    {
        get => hexDump;
        set { hexDump = value; OnPropertyChanged(); }
    }
    public event PropertyChangedEventHandler? PropertyChanged;
    public void SetPacket (PacketModel packet) {
        if (packet?.RawData == null) {
            HexDump = "";
            return;
        }

        var sb = new StringBuilder();

        for (int i = 0; i < packet.RawData.Length; i++) {
            if (i % 16 == 0)
                sb.AppendLine();

            sb.Append(packet.RawData[i].ToString("X2") + " ");
        }

        HexDump = sb.ToString();
    }
    private void OnPropertyChanged ([System.Runtime.CompilerServices.CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}
