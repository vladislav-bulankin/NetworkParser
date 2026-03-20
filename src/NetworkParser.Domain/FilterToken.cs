using NetworkParser.Domain.Enums;

namespace NetworkParser.Domain;

public class FilterToken {
    public FilterTokenType Type { get; }
    public string Value { get; }

    public FilterToken (FilterTokenType type, string value) {
        Type = type;
        Value = value;
    }
}
