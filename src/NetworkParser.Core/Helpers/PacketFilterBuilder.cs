using NetworkParser.Domain.Enums;
using NetworkParser.Domain.Packets;

namespace NetworkParser.Core.Helpers;

public static class PacketFilterBuilder {
    public static Func<PacketModel, bool> Build (string filter) {
        if (string.IsNullOrWhiteSpace(filter)){ return _ => true; }
        var tokens = FilterTokenizer.Tokenize(filter);
        return packet => {
            bool result = true;
            bool useOr = false;
            for (int i = 0; i < tokens.Count; i++) {
                var t = tokens[i];
                bool current = false;
                if (t.Type == FilterTokenType.Identifier) {
                    current = EvaluateIdentifier(packet, t.Value);
                } else if (t.Type == FilterTokenType.Identifier && i + 2 < tokens.Count) {
                    var op = tokens[i + 1];
                    var val = tokens[i + 2];

                    current = EvaluateExpression(packet, t.Value, op.Value, val.Value);
                    i += 2;
                } else if (t.Type == FilterTokenType.And) {
                    useOr = false;
                    continue;
                } else if (t.Type == FilterTokenType.Or) {
                    useOr = true;
                    continue;
                }

                result = useOr ? result || current : result && current;
            }

            return result;
        };
    }
    private static bool EvaluateIdentifier (PacketModel p, string id) {
        id = id.ToLower();
        return id switch {
            "tcp" => p.Protocol == "Tcp",
            "udp" => p.Protocol == "Udp",
            "http" => p.Info?.Contains("HTTP") ?? false,
            _ => false
        };
    }

    private static bool EvaluateExpression (PacketModel p, string field, string op, string value) {
        field = field.ToLower();
        return field switch {
            "ip" => Compare(p.Source, op, value) || Compare(p.Destination, op, value),
            "port" => Compare(p.Info, op, value),
            "len" => Compare(p.Length.ToString(), op, value),
            "tcp.port" => Compare(p.Info, op, value),
            _ => false
        };
    }

    private static bool Compare (string? field, string op, string value) {
        if (field == null) { return false; }
        return op switch {
            "==" => field.Contains(value),
            ">" => int.TryParse(field, out var f) && int.TryParse(value, out var v) && f > v,
            "<" => int.TryParse(field, out var f2) && int.TryParse(value, out var v2) && f2 < v2,
            _ => false
        };
    }
}
