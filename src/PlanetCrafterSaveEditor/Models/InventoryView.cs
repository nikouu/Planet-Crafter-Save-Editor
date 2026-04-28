using System.Text.Json.Nodes;

namespace PlanetCrafterSaveEditor.Models;

public sealed class InventoryView
{
    private readonly SaveRecord _record;
    private readonly JsonObject _json;

    public InventoryView(SaveRecord record)
    {
        _record = record;
        _json = record.GetJson();
    }

    public SaveRecord Record => _record;

    public long Id => _json["id"]!.GetValue<long>();

    public int Size => _json["size"]!.GetValue<int>();

    public string WoIdsRaw => _json["woIds"]!.GetValue<string>();

    public IReadOnlyList<long> WoIds
    {
        get
        {
            var raw = WoIdsRaw;
            if (raw.Length == 0)
            {
                return Array.Empty<long>();
            }
            var parts = raw.Split(',');
            var ids = new long[parts.Length];
            for (var i = 0; i < parts.Length; i++)
            {
                ids[i] = long.Parse(parts[i]);
            }
            return ids;
        }
    }
}
