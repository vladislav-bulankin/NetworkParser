using NetworkParser.Domain;
using NetworkParser.Domain.Enums;

namespace NetworkParser.Core.Helpers;

public static class FilterTokenizer {
    public static List<FilterToken> Tokenize (string input) {
        var tokens = new List<FilterToken>();

        if (string.IsNullOrWhiteSpace(input))
            return tokens;

        var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts) {
            if (part == "&&")
                tokens.Add(new FilterToken(FilterTokenType.And, part));
            else if (part == "||")
                tokens.Add(new FilterToken(FilterTokenType.Or, part));
            else if (part == "==" || part == ">" || part == "<")
                tokens.Add(new FilterToken(FilterTokenType.Operator, part));
            else if (char.IsDigit(part[0]))
                tokens.Add(new FilterToken(FilterTokenType.Value, part));
            else
                tokens.Add(new FilterToken(FilterTokenType.Identifier, part));
        }

        return tokens;
    }
}
