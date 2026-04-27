using System.Text.Json.Nodes;

namespace PlanetCrafterSaveEditor.Models;

public sealed class SaveMetaView
{
    private readonly SaveRecord _record;
    private readonly JsonObject _json;

    public SaveMetaView(SaveRecord record)
    {
        _record = record;
        _json = JsonNode.Parse(record.OriginalText) as JsonObject
            ?? throw new FormatException("SaveMeta record is not a JSON object.");
    }

    public SaveRecord Record => _record;

    public string SaveDisplayName
        => _json.TryGetPropertyValue("saveDisplayName", out var v) && v is not null
            ? v.GetValue<string>()
            : string.Empty;

    public string PlanetId
        => _json.TryGetPropertyValue("planetId", out var v) && v is not null
            ? v.GetValue<string>()
            : string.Empty;

    public string Version
        => _json.TryGetPropertyValue("version", out var v) && v is not null
            ? v.GetValue<string>()
            : string.Empty;

    public string Mode
        => _json.TryGetPropertyValue("mode", out var v) && v is not null
            ? v.GetValue<string>()
            : string.Empty;
}
