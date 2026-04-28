using System.Text.Json.Nodes;

namespace PlanetCrafterSaveEditor.Models;

public sealed class SaveFile
{
    public IList<SaveRecord> Records { get; init; } = new List<SaveRecord>();

    public IList<string> Separators { get; init; } = new List<string>();
}

public sealed class SaveRecord
{
    public required string OriginalText { get; set; }

    public string? OriginalSnapshot { get; set; }

    public bool IsDirty { get; set; }

    private JsonObject? _parsedCache;
    private string? _parsedOf;

    public JsonObject GetJson()
    {
        if (_parsedCache is not null && ReferenceEquals(_parsedOf, OriginalText))
        {
            return _parsedCache;
        }
        var parsed = JsonNode.Parse(OriginalText) as JsonObject
            ?? throw new FormatException("Record is not a JSON object.");
        _parsedCache = parsed;
        _parsedOf = OriginalText;
        return parsed;
    }
}
