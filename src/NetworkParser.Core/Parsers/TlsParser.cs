using NetworkParser.Core.Abstractions.Parsers;

namespace NetworkParser.Core.Parsers;

public class TlsParser : ITlsParser {
    public string? ExtractSni (byte[] payload) {
        try {
            // TLS Record Layer
            if (payload.Length < 5) { return null; }
               
            if (payload[0] != 0x16) { return null; } // Content Type: Handshake

            // Handshake Type: ClientHello = 0x01
            if (payload[5] != 0x01) { return null; }

            int pos = 43; // пропускаем фиксированные поля ClientHello

            if (pos >= payload.Length) { return null; }
            // Session ID
            int sessionIdLen = payload[pos++];
            pos += sessionIdLen;

            if (pos + 2 >= payload.Length) { return null; }
            // Cipher Suites
            int cipherLen = (payload[pos] << 8) | payload[pos + 1];
            pos += 2 + cipherLen;

            if (pos + 1 >= payload.Length) { return null; }
            // Compression Methods
            int compLen = payload[pos++];
            pos += compLen;

            if (pos + 2 >= payload.Length) { return null; }
            // Extensions
            int extTotalLen = (payload[pos] << 8) | payload[pos + 1];
            pos += 2;

            int extEnd = pos + extTotalLen;

            while (pos + 4 < extEnd && pos + 4 < payload.Length) {
                int extType = (payload[pos] << 8) | payload[pos + 1];
                int extLen = (payload[pos + 2] << 8) | payload[pos + 3];
                pos += 4;

                if (extType == 0x0000) { // SNI extension
                    // SNI List Length
                    pos += 2;
                    // Name Type (0x00 = host_name)
                    pos += 1;
                    // Name Length
                    int nameLen = (payload[pos] << 8) | payload[pos + 1];
                    pos += 2;
                    return System.Text.Encoding.ASCII.GetString(payload, pos, nameLen);
                }

                pos += extLen;
            }
        } catch { /* malformed packet */ }
        return null;
    }
}
