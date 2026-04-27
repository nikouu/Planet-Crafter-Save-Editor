using PlanetCrafterSaveEditor.Models;
using PlanetCrafterSaveEditor.Parsing;
using PlanetCrafterSaveEditor.Services;

namespace PlanetCrafterSaveEditor.Tests;

public class CategorizerTests
{
    private static WorldObjectView Make(string gId, bool withPos = false)
    {
        var json = withPos
            ? $"{{\"id\":1,\"gId\":\"{gId}\",\"pos\":\"0,0,0\"}}"
            : $"{{\"id\":1,\"gId\":\"{gId}\"}}";
        var record = new SaveRecord { OriginalText = json };
        return new WorldObjectView(record);
    }

    [Theory]
    [InlineData("DeathChest", WorldObjectCategory.DeathChest)]
    [InlineData("DeathChestPlayer", WorldObjectCategory.DeathChest)]
    [InlineData("ChestT1", WorldObjectCategory.Container)]
    [InlineData("WreckLocker", WorldObjectCategory.Container)]
    [InlineData("StorageBoxLarge", WorldObjectCategory.Container)]
    [InlineData("Rover1", WorldObjectCategory.Vehicle)]
    [InlineData("DroneStation", WorldObjectCategory.Vehicle)]
    public void Categorize_PatternsMatch(string gId, WorldObjectCategory expected)
    {
        var cat = WorldObjectCategorizer.Default();
        Assert.Equal(expected, cat.Categorize(Make(gId)));
    }

    [Fact]
    public void Categorize_DropOnlyWhenHasPositionAndNoMatch()
    {
        var cat = WorldObjectCategorizer.Default();
        Assert.Equal(WorldObjectCategory.Drop, cat.Categorize(Make("astrofood", withPos: true)));
        Assert.Equal(WorldObjectCategory.Other, cat.Categorize(Make("astrofood", withPos: false)));
    }
}
