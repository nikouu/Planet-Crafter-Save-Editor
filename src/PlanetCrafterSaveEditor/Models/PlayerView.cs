using System.Text.Json;
using System.Text.Json.Nodes;

namespace PlanetCrafterSaveEditor.Models;

public sealed class PlayerView
{
    private readonly SaveRecord _record;
    private readonly JsonObject _json;

    public PlayerView(SaveRecord record)
    {
        _record = record;
        _json = record.GetJson();
    }

    public SaveRecord Record => _record;

    public long Id => _json["id"]!.GetValue<long>();

    public string Name => _json["name"]!.GetValue<string>();

    public int InventoryId => _json["inventoryId"]!.GetValue<int>();

    public int EquipmentId => _json["equipmentId"]!.GetValue<int>();

    public bool IsHost => _json.TryGetPropertyValue("host", out var h) && h is not null && h.GetValue<bool>();

    public string PlanetId => _json["planetId"]!.GetValue<string>();

    public Vec3 Position => Vec3.Parse(_json["playerPosition"]!.GetValue<string>());

    public string PositionString => _json["playerPosition"]!.GetValue<string>();
}
