namespace NetworkParser.Domain.Packets; 
public class PacketModel {
    public int Number { get; set; }
    public DateTime Timestamp { get; set; }
    public string Source { get; set; }
    public string Destination { get; set; }
    public string Protocol { get; set; }
    public int Length { get; set; }
    public string Info { get; set; }

    public byte[] RawData { get; set; }
}
