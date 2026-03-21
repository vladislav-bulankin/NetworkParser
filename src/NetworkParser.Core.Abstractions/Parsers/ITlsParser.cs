namespace NetworkParser.Core.Abstractions.Parsers;

public interface ITlsParser {
    string? ExtractSni (byte[] payload);
}
