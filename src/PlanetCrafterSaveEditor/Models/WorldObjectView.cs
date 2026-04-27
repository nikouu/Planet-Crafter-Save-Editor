using System.Text.Json.Nodes;

namespace PlanetCrafterSaveEditor.Models;

public sealed class WorldObjectView
{
    private readonly SaveRecord _record;
    private readonly JsonObject _json;

    public WorldObjectView(SaveRecord record)
    {
        _record = record;
        _json = JsonNode.Parse(record.OriginalText) as JsonObject
            ?? throw new FormatException("WorldObject record is not a JSON object.");
    }

    public SaveRecord Record => _record;

    public long Id => _json["id"]!.GetValue<long>();

    public string GId => _json["gId"]!.GetValue<string>();

    public bool HasPosition => _json.ContainsKey("pos");

    public Vec3? Position
        => _json.TryGetPropertyValue("pos", out var p) && p is not null
            ? Vec3.Parse(p.GetValue<string>())
            : null;

    public string? PositionString
        => _json.TryGetPropertyValue("pos", out var p) && p is not null
            ? p.GetValue<string>()
            : null;

    public long? PlanetHash
        => _json.TryGetPropertyValue("planet", out var p) && p is not null
            ? p.GetValue<long>()
            : null;

    public bool HasCount => _json.ContainsKey("count");

    public string? Count
        => _json.TryGetPropertyValue("count", out var c) && c is not null
            ? c.GetValue<string>()
            : null;

    public bool HasGrowth => _json.ContainsKey("grwth");

    public int? Growth
        => _json.TryGetPropertyValue("grwth", out var g) && g is not null
            ? g.GetValue<int>()
            : null;
}
