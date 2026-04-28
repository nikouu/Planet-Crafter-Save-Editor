using System.Text.RegularExpressions;
using PlanetCrafterSaveEditor.Models;

namespace PlanetCrafterSaveEditor.Services;

internal static class RecordFieldEditor
{
    public static void SetStringField(SaveRecord record, string fieldName, string newValue)
    {
        var pattern = "\"" + Regex.Escape(fieldName) + "\"\\s*:\\s*\"(?:[^\"\\\\]|\\\\.)*\"";
        var replacement = "\"" + fieldName + "\":\"" + EscapeJsonString(newValue) + "\"";
        var regex = new Regex(pattern, RegexOptions.None, TimeSpan.FromSeconds(1));
        if (!regex.IsMatch(record.OriginalText))
        {
            throw new InvalidOperationException($"Field '{fieldName}' not found in record.");
        }
        var updated = regex.Replace(record.OriginalText, replacement);
        if (updated == record.OriginalText)
        {
            // Field exists but value unchanged — no-op.
            return;
        }
        record.OriginalSnapshot ??= record.OriginalText;
        record.OriginalText = updated;
        record.IsDirty = true;
    }

    public static void SetNumberField(SaveRecord record, string fieldName, string newLiteral)
    {
        var pattern = "\"" + Regex.Escape(fieldName) + "\"\\s*:\\s*-?\\d+(?:\\.\\d+)?(?:[eE][+-]?\\d+)?";
        var replacement = "\"" + fieldName + "\":" + newLiteral;
        var regex = new Regex(pattern, RegexOptions.None, TimeSpan.FromSeconds(1));
        if (!regex.IsMatch(record.OriginalText))
        {
            throw new InvalidOperationException($"Field '{fieldName}' not found in record.");
        }
        var updated = regex.Replace(record.OriginalText, replacement);
        if (updated == record.OriginalText)
        {
            return;
        }
        record.OriginalSnapshot ??= record.OriginalText;
        record.OriginalText = updated;
        record.IsDirty = true;
    }

    private static string EscapeJsonString(string s)
    {
        var sb = new System.Text.StringBuilder(s.Length);
        foreach (var c in s)
        {
            switch (c)
            {
                case '\\': sb.Append("\\\\"); break;
                case '"': sb.Append("\\\""); break;
                case '\b': sb.Append("\\b"); break;
                case '\f': sb.Append("\\f"); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (c < 0x20)
                    {
                        sb.Append("\\u").Append(((int)c).ToString("x4"));
                    }
                    else
                    {
                        sb.Append(c);
                    }
                    break;
            }
        }
        return sb.ToString();
    }
}
