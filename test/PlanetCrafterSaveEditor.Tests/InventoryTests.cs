using PlanetCrafterSaveEditor.Models;
using PlanetCrafterSaveEditor.Parsing;
using PlanetCrafterSaveEditor.Services;

namespace PlanetCrafterSaveEditor.Tests;

public class InventoryTests
{
    private static SaveSession Load(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "exampleSaves", fileName);
        Skip.IfNot(File.Exists(path), $"Example save not present: {path}");
        var session = new SaveSession();
        session.Load(File.ReadAllBytes(path), fileName);
        return session;
    }

    [SkippableFact]
    public void RemoveFromInventory_DropsWoIdAndDeletesWorldObject()
    {
        var session = Load("ToxicityPlanetTest.json");
        var inv = session.Save!.Inventories().First(i => i.WoIds.Count > 0);
        var firstWo = inv.WoIds[0];
        var originalCount = inv.WoIds.Count;
        var originalWoCount = session.Save!.WorldObjects().Count();

        session.RemoveFromInventory(inv.Id, firstWo);

        var updatedInv = session.Save!.Inventories().First(i => i.Id == inv.Id);
        Assert.Equal(originalCount - 1, updatedInv.WoIds.Count);
        Assert.DoesNotContain(firstWo, updatedInv.WoIds);
        Assert.DoesNotContain(session.Save!.WorldObjects(), w => w.Id == firstWo);
        Assert.Equal(originalWoCount - 1, session.Save!.WorldObjects().Count());
        Assert.True(session.IsDirty);
    }

    [SkippableFact]
    public void MoveBetweenInventories_TransfersIdAndPreservesOthers()
    {
        var session = Load("ToxicityPlanetTest.json");
        var invs = session.Save!.Inventories().Where(i => i.WoIds.Count > 0).ToList();
        var from = invs[0];
        var to = session.Save!.Inventories().First(i => i.Id != from.Id && i.WoIds.Count < i.Size);
        var moved = from.WoIds[0];
        var fromBefore = from.WoIds.Count;
        var toBefore = to.WoIds.Count;

        session.MoveBetweenInventories(from.Id, to.Id, moved);

        var fromAfter = session.Save!.Inventories().First(i => i.Id == from.Id);
        var toAfter = session.Save!.Inventories().First(i => i.Id == to.Id);
        Assert.Equal(fromBefore - 1, fromAfter.WoIds.Count);
        Assert.Equal(toBefore + 1, toAfter.WoIds.Count);
        Assert.DoesNotContain(moved, fromAfter.WoIds);
        Assert.Contains(moved, toAfter.WoIds);
    }

    [SkippableFact]
    public void AddToInventory_InsertsNewWorldObjectAndAppendsToCsv()
    {
        var session = Load("ToxicityPlanetTest.json");
        var inv = session.Save!.Inventories().First(i => i.WoIds.Count < i.Size);
        var beforeWoCount = session.Save!.WorldObjects().Count();
        var beforeIds = inv.WoIds.Count;

        var newId = session.AddToInventory(inv.Id, "TrashMetalsScraps1");

        var updatedInv = session.Save!.Inventories().First(i => i.Id == inv.Id);
        Assert.Equal(beforeIds + 1, updatedInv.WoIds.Count);
        Assert.Contains(newId, updatedInv.WoIds);
        Assert.Equal(beforeWoCount + 1, session.Save!.WorldObjects().Count());

        var newWo = session.Save!.WorldObjects().First(w => w.Id == newId);
        Assert.Equal("TrashMetalsScraps1", newWo.GId);
        Assert.False(newWo.HasPosition);
    }

    [SkippableFact]
    public void AddToInventory_FullInventory_Throws()
    {
        var session = Load("ToxicityPlanetTest.json");
        var inv = session.Save!.Inventories().FirstOrDefault(i => i.WoIds.Count >= i.Size);
        Skip.If(inv is null, "No full inventory in save.");
        Assert.Throws<InvalidOperationException>(() =>
            session.AddToInventory(inv!.Id, "TrashMetalsScraps1"));
    }

    [SkippableFact]
    public void SetGrowth_UpdatesGrwthField()
    {
        var session = Load("ToxicityPlanetTest.json");
        var wo = session.Save!.WorldObjects().First(w => w.HasGrowth);
        var oldGrowth = wo.Growth;

        session.SetGrowth(wo.Id, 50);

        var updated = session.Save!.WorldObjects().First(w => w.Id == wo.Id);
        Assert.Equal(50, updated.Growth);
        Assert.NotEqual(oldGrowth, updated.Growth);
    }

    [SkippableFact]
    public void SetCount_UpdatesCountField()
    {
        var session = Load("ToxicityPlanetTest.json");
        var wo = session.Save!.WorldObjects().First(w => w.HasCount);
        var oldCount = wo.Count;

        session.SetCount(wo.Id, "42,99");

        var updated = session.Save!.WorldObjects().First(w => w.Id == wo.Id);
        Assert.Equal("42,99", updated.Count);
        Assert.NotEqual(oldCount, updated.Count);
    }

    [SkippableFact]
    public void EditedSave_RoundTripsThroughReader()
    {
        var session = Load("ToxicityPlanetTest.json");
        var inv = session.Save!.Inventories().First(i => i.WoIds.Count > 0);
        var removeId = inv.WoIds[0];

        session.RemoveFromInventory(inv.Id, removeId);
        var bytes = session.Write();

        // Re-parse the written bytes and confirm the WO is gone and inventory's CSV is shorter
        var reloaded = SaveFileReader.Read(bytes);
        Assert.DoesNotContain(reloaded.WorldObjects(), w => w.Id == removeId);
        var reloadedInv = reloaded.Inventories().First(i => i.Id == inv.Id);
        Assert.DoesNotContain(removeId, reloadedInv.WoIds);
    }
}
