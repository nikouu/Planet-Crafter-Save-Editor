using PlanetCrafterSaveEditor.Models;
using PlanetCrafterSaveEditor.Parsing;

namespace PlanetCrafterSaveEditor.Tests;

public class TypedViewTests
{
    private static SaveFile LoadOrSkip(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "exampleSaves", fileName);
        Skip.IfNot(File.Exists(path), $"Example save not present: {path}");
        return SaveFileReader.Read(File.ReadAllBytes(path));
    }

    [SkippableFact]
    public void Prime_HasOnePlayer_AtExpectedPosition()
    {
        var save = LoadOrSkip("PrimePlanetTest.json");
        var players = save.Players().ToList();

        Assert.Single(players);
        var p = players[0];
        Assert.Equal("Player-1", p.Name);
        Assert.Equal("Prime", p.PlanetId);
        Assert.True(p.IsHost);
        Assert.Equal(3, p.InventoryId);
        Assert.Equal(4, p.EquipmentId);
        Assert.Equal("406.2525,7.150435,597.8572", p.PositionString);
        var pos = p.Position;
        Assert.Equal(406.2525f, pos.X, 4);
        Assert.Equal(7.150435f, pos.Y, 4);
        Assert.Equal(597.8572f, pos.Z, 4);
    }

    [SkippableFact]
    public void Toxicity_HasThreePlayers_HostFirst()
    {
        var save = LoadOrSkip("ToxicityPlanetTest.json");
        var players = save.Players().ToList();

        Assert.Equal(3, players.Count);
        Assert.True(players[0].IsHost);
        Assert.False(players[1].IsHost);
        Assert.False(players[2].IsHost);
        Assert.All(players, p => Assert.Equal("Toxicity", p.PlanetId));
    }

    [SkippableFact]
    public void Prime_HasOneWorldObject_TheEscapePod()
    {
        var save = LoadOrSkip("PrimePlanetTest.json");
        var wos = save.WorldObjects().ToList();

        Assert.Single(wos);
        var wo = wos[0];
        Assert.Equal("EscapePod", wo.GId);
        Assert.True(wo.HasPosition);
        Assert.Equal("414.61,8.3,585.28", wo.PositionString);
        Assert.Equal(-1140328421L, wo.PlanetHash);
    }

    [SkippableFact]
    public void Humble_EscapePodGId_IsSuffixed()
    {
        var save = LoadOrSkip("HumblePlanetTest.json");
        var wo = save.WorldObjects().Single();
        Assert.Equal("EscapePodHumble", wo.GId);
        Assert.Equal(-486276833L, wo.PlanetHash);
    }

    [SkippableFact]
    public void Prime_HasFourInventories_AllEmpty()
    {
        var save = LoadOrSkip("PrimePlanetTest.json");
        var invs = save.Inventories().ToList();

        Assert.Equal(4, invs.Count);
        Assert.Equal(new long[] { 1, 2, 3, 4 }, invs.Select(i => i.Id).ToArray());
        Assert.All(invs, i => Assert.Empty(i.WoIds));
    }

    [SkippableFact]
    public void Prime_SaveMeta_HasExpectedFields()
    {
        var save = LoadOrSkip("PrimePlanetTest.json");
        var meta = save.SaveMeta();

        Assert.NotNull(meta);
        Assert.Equal("PrimePlanetTest", meta!.SaveDisplayName);
        Assert.Equal("Prime", meta.PlanetId);
        Assert.Equal("2.007", meta.Version);
        Assert.Equal("Standard", meta.Mode);
    }

    [SkippableFact]
    public void AllExampleSaves_ClassifyKnownSections()
    {
        foreach (var name in new[] { "PrimePlanetTest.json", "Prime2.json", "HumblePlanetTest.json", "SelenaPlanetTest.json", "ToxicityPlanetTest.json" })
        {
            var path = Path.Combine(AppContext.BaseDirectory, "exampleSaves", name);
            if (!File.Exists(path)) continue;

            var save = SaveFileReader.Read(File.ReadAllBytes(path));
            Assert.NotEmpty(save.Players());
            Assert.NotEmpty(save.WorldObjects());
            Assert.NotEmpty(save.Inventories());
            Assert.NotNull(save.SaveMeta());
        }
    }
}

public class Vec3Tests
{
    [Fact]
    public void Parse_ThenToString_RoundTripsOriginalString()
    {
        var v = Vec3.Parse("406.2525,7.150435,597.8572");
        Assert.Equal("406.2525,7.150435,597.8572", v.ToString());
    }

    [Fact]
    public void Constructor_FormatsWithInvariantCulture()
    {
        var v = new Vec3(1.5f, 2.25f, 3.125f);
        Assert.Equal("1.5,2.25,3.125", v.ToString());
    }

    [Fact]
    public void TryParse_RejectsBadInput()
    {
        Assert.False(Vec3.TryParse(null, out _));
        Assert.False(Vec3.TryParse("1,2", out _));
        Assert.False(Vec3.TryParse("a,b,c", out _));
    }
}
