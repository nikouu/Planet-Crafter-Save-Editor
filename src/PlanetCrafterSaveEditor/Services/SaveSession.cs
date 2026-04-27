using System.Globalization;
using PlanetCrafterSaveEditor.Models;
using PlanetCrafterSaveEditor.Parsing;

namespace PlanetCrafterSaveEditor.Services;

public sealed class SaveSession
{
    public SaveFile? Save { get; private set; }

    public string? FileName { get; private set; }

    public event Action? OnChanged;

    public bool IsDirty => Save is not null && Save.Records.Any(r => r.IsDirty);

    public void Load(byte[] bytes, string fileName)
    {
        Save = SaveFileReader.Read(bytes);
        FileName = fileName;
        Notify();
    }

    public byte[] Write()
    {
        if (Save is null)
        {
            throw new InvalidOperationException("No save loaded.");
        }
        return SaveFileWriter.Write(Save);
    }

    public void TeleportPlayer(long playerId, Vec3 destination)
    {
        var save = RequireSave();
        var player = save.Players().FirstOrDefault(p => p.Id == playerId)
            ?? throw new InvalidOperationException($"Player {playerId} not found.");
        RecordFieldEditor.SetStringField(player.Record, "playerPosition", destination.ToString());
        Notify();
    }

    public void TeleportWorldObject(long worldObjectId, Vec3 destination)
    {
        var save = RequireSave();
        var wo = save.WorldObjects().FirstOrDefault(w => w.Id == worldObjectId)
            ?? throw new InvalidOperationException($"WorldObject {worldObjectId} not found.");
        if (!wo.HasPosition)
        {
            throw new InvalidOperationException($"WorldObject {worldObjectId} has no position to teleport.");
        }
        RecordFieldEditor.SetStringField(wo.Record, "pos", destination.ToString());
        Notify();
    }

    public void RemoveFromInventory(long inventoryId, long worldObjectId)
    {
        var save = RequireSave();
        var inv = save.Inventories().FirstOrDefault(i => i.Id == inventoryId)
            ?? throw new InvalidOperationException($"Inventory {inventoryId} not found.");
        if (!inv.WoIds.Contains(worldObjectId))
        {
            throw new InvalidOperationException($"Inventory {inventoryId} does not contain woId {worldObjectId}.");
        }

        // Defensive: refuse if the wo id is referenced from another inventory too
        var otherRefs = save.Inventories()
            .Where(i => i.Id != inventoryId && i.WoIds.Contains(worldObjectId))
            .ToList();
        if (otherRefs.Count > 0)
        {
            throw new InvalidOperationException(
                $"WorldObject {worldObjectId} is also referenced by inventory {otherRefs[0].Id}; refusing to remove.");
        }

        var newCsv = string.Join(",", inv.WoIds.Where(x => x != worldObjectId));
        RecordFieldEditor.SetStringField(inv.Record, "woIds", newCsv);

        // Delete the WO record
        var woIndex = FindWorldObjectRecordIndex(save, worldObjectId);
        if (woIndex >= 0)
        {
            SaveFileEditor.RemoveRecordAt(save, woIndex);
        }

        Notify();
    }

    public void MoveBetweenInventories(long fromInventoryId, long toInventoryId, long worldObjectId)
    {
        if (fromInventoryId == toInventoryId)
        {
            throw new InvalidOperationException("Source and target inventories must differ.");
        }
        var save = RequireSave();
        var from = save.Inventories().FirstOrDefault(i => i.Id == fromInventoryId)
            ?? throw new InvalidOperationException($"Inventory {fromInventoryId} not found.");
        var to = save.Inventories().FirstOrDefault(i => i.Id == toInventoryId)
            ?? throw new InvalidOperationException($"Inventory {toInventoryId} not found.");
        if (!from.WoIds.Contains(worldObjectId))
        {
            throw new InvalidOperationException($"Inventory {fromInventoryId} does not contain woId {worldObjectId}.");
        }
        if (to.WoIds.Count >= to.Size)
        {
            throw new InvalidOperationException($"Inventory {toInventoryId} is full.");
        }

        var fromCsv = string.Join(",", from.WoIds.Where(x => x != worldObjectId));
        RecordFieldEditor.SetStringField(from.Record, "woIds", fromCsv);

        var toIds = to.WoIds.ToList();
        toIds.Add(worldObjectId);
        var toCsv = string.Join(",", toIds);
        RecordFieldEditor.SetStringField(to.Record, "woIds", toCsv);

        Notify();
    }

    public long AddToInventory(long inventoryId, string gId)
    {
        var save = RequireSave();
        var inv = save.Inventories().FirstOrDefault(i => i.Id == inventoryId)
            ?? throw new InvalidOperationException($"Inventory {inventoryId} not found.");
        if (inv.WoIds.Count >= inv.Size)
        {
            throw new InvalidOperationException($"Inventory {inventoryId} is full.");
        }
        if (string.IsNullOrWhiteSpace(gId))
        {
            throw new ArgumentException("gId is required.", nameof(gId));
        }

        var newId = AllocateUniqueWoId(save);
        var newRecord = new SaveRecord
        {
            OriginalText = $"{{\"id\":{newId.ToString(CultureInfo.InvariantCulture)},\"gId\":\"{EscapeJson(gId)}\"}}",
            IsDirty = true,
        };
        SaveFileEditor.AppendToSection(save, SaveSection.WorldObjects, newRecord);

        var newCsv = inv.WoIds.Count == 0
            ? newId.ToString(CultureInfo.InvariantCulture)
            : inv.WoIdsRaw + "," + newId.ToString(CultureInfo.InvariantCulture);
        RecordFieldEditor.SetStringField(inv.Record, "woIds", newCsv);

        Notify();
        return newId;
    }

    public void SetCount(long worldObjectId, string countLiteral)
    {
        var save = RequireSave();
        var wo = save.WorldObjects().FirstOrDefault(w => w.Id == worldObjectId)
            ?? throw new InvalidOperationException($"WorldObject {worldObjectId} not found.");
        if (!wo.HasCount)
        {
            throw new InvalidOperationException($"WorldObject {worldObjectId} has no count field.");
        }
        RecordFieldEditor.SetStringField(wo.Record, "count", countLiteral);
        Notify();
    }

    public void SetGrowth(long worldObjectId, int growth)
    {
        var save = RequireSave();
        var wo = save.WorldObjects().FirstOrDefault(w => w.Id == worldObjectId)
            ?? throw new InvalidOperationException($"WorldObject {worldObjectId} not found.");
        if (!wo.HasGrowth)
        {
            throw new InvalidOperationException($"WorldObject {worldObjectId} has no grwth field.");
        }
        RecordFieldEditor.SetNumberField(wo.Record, "grwth", growth.ToString(CultureInfo.InvariantCulture));
        Notify();
    }

    private static int FindWorldObjectRecordIndex(SaveFile save, long worldObjectId)
    {
        foreach (var (idx, section, record) in save.EnumerateBySection())
        {
            if (section != SaveSection.WorldObjects) continue;
            var view = new WorldObjectView(record);
            if (view.Id == worldObjectId)
            {
                return idx;
            }
        }
        return -1;
    }

    private static long AllocateUniqueWoId(SaveFile save)
    {
        var used = new HashSet<long>();
        foreach (var wo in save.WorldObjects())
        {
            used.Add(wo.Id);
        }
        foreach (var inv in save.Inventories())
        {
            used.Add(inv.Id);
            foreach (var id in inv.WoIds) used.Add(id);
        }

        var rng = new Random();
        for (var attempt = 0; attempt < 1000; attempt++)
        {
            var candidate = (long)rng.Next(100_000_000, 1_000_000_000);
            if (!used.Contains(candidate))
            {
                return candidate;
            }
        }
        throw new InvalidOperationException("Could not allocate a unique WO id after 1000 attempts.");
    }

    private static string EscapeJson(string s)
        => s.Replace("\\", "\\\\").Replace("\"", "\\\"");

    private SaveFile RequireSave()
        => Save ?? throw new InvalidOperationException("No save loaded.");

    private void Notify() => OnChanged?.Invoke();
}
