namespace NetworkParser.Domain.Stat;

public class StatRow {
    public string Label { get; }
    public int Count { get; }
    public string Percent { get; }
    public double BarValue { get; }
    public StatRow (string label, int count, int total) {
        Label = label;
        Count = count;
        Percent = $"{count * 100.0 / total:F1}%";
        BarValue = count * 100.0 / total;
    }
}
