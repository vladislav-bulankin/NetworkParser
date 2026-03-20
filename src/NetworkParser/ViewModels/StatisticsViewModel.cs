using System.Collections.ObjectModel;
using System.ComponentModel;
using NetworkParser.Domain.Packets;
using NetworkParser.Domain.Stat;

namespace NetworkParser.UI.ViewModels;

public class StatisticsViewModel : INotifyPropertyChanged {
    public event PropertyChangedEventHandler? PropertyChanged;
    public string TotalPackets { get; private set; } = "";
    public string CaptureDuration { get; private set; } = "";
    public string TotalBytes { get; private set; } = "";
    public string AvgPacketSize { get; private set; } = "";
    public string TotalPacketsLabel => $"Packets: {TotalPackets}";
    public string TotalBytesLabel => $"Total: {TotalBytes}";
    public string DurationLabel => $"Duration: {CaptureDuration}";
    public string AvgSizeLabel => $"Avg size: {AvgPacketSize}";
    public ObservableCollection<StatRow> TopProtocols { get; } = new();
    public ObservableCollection<StatRow> TopSources { get; } = new();
    public ObservableCollection<StatRow> TopConversations { get; } = new();

    public void Build (IEnumerable<PacketModel> packets) {
        var list = packets.ToList();
        if (list.Count == 0){ return; }
        // Общая инфо
        TotalPackets = list.Count.ToString();
        TotalBytes = $"{list.Sum(p => p.Length):N0} bytes";
        AvgPacketSize = $"{list.Average(p => p.Length):F0} bytes";
        var duration = list.Max(p => p.Timestamp) - list.Min(p => p.Timestamp);
        CaptureDuration = $"{duration.TotalSeconds:F1} sec";

        // Топ протоколов
        var protocols = list
            .GroupBy(p => p.Protocol ?? "Unknown")
            .OrderByDescending(g => g.Count())
            .Take(10);
        foreach (var g in protocols){
            TopProtocols.Add(new StatRow(g.Key, g.Count(), list.Count));
        }

        // Топ IP источников
        var sources = list
            .Where(p => !string.IsNullOrEmpty(p.Source))
            .GroupBy(p => p.Source)
            .OrderByDescending(g => g.Count())
            .Take(10);
        foreach (var g in sources){
            TopSources.Add(new StatRow(g.Key, g.Count(), list.Count));
        }

        // Топ conversations
        var conversations = list
            .Where(p => !string.IsNullOrEmpty(p.Source) && !string.IsNullOrEmpty(p.Destination))
            .GroupBy(p => {
                var a = p.Source;
                var b = p.Destination;
                return string.Compare(a, b) < 0 ? $"{a} ↔ {b}" : $"{b} ↔ {a}";
            })
            .OrderByDescending(g => g.Count())
            .Take(10);
        foreach (var g in conversations){
            TopConversations.Add(new StatRow(g.Key, g.Count(), list.Count));
        }
    }
}
