using PlanetCrafterSaveEditor.Models;
using PlanetCrafterSaveEditor.Parsing;
using PlanetCrafterSaveEditor.Services;

namespace PlanetCrafterSaveEditor.Tests;

public class SpawnResolverTests
{
    private static readonly Dictionary<string, long> ShippedHashes = new()
    {
        ["Prime"] = -1140328421,
        ["Humble"] = -486276833,
        ["Selenea"] = -1016990411,
        ["Toxicity"] = 110910045,
    };

    private static SpawnResolver Resolver() => new(ShippedHashes);

    private static SaveFile LoadOrSkip(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "exampleSaves", fileName);
        Skip.IfNot(File.Exists(path), $"Example save not present: {path}");
        return SaveFileReader.Read(File.ReadAllBytes(path));
    }

    [SkippableTheory]
    [InlineData("HumblePlanetTest.json", "Humble", "EscapePodHumble")]
    [InlineData("ToxicityPlanetTest.json", "Toxicity", "EscapePodToxicity")]
    public void ResolveSpawn_Suffixed_FindsPod(string file, string planet, string expectedGId)
    {
        var save = LoadOrSkip(file);
        var pod = Resolver().ResolveSpawn(save, planet);
        Assert.NotNull(pod);
        Assert.Equal(expectedGId, pod!.GId);
    }

    [SkippableTheory]
    [InlineData("PrimePlanetTest.json", "Prime")]
    [InlineData("Prime2.json", "Prime")]
    [InlineData("SelenaPlanetTest.json", "Selenea")]
    public void ResolveSpawn_BareGId_FindsPodByPlanetHash(string file, string planet)
    {
        var save = LoadOrSkip(file);
        var pod = Resolver().ResolveSpawn(save, planet);
        Assert.NotNull(pod);
        Assert.Equal("EscapePod", pod!.GId);
        Assert.Equal(ShippedHashes[planet], pod.PlanetHash);
    }

    [Fact]
    public void ResolveSpawn_NoEscapePod_ReturnsNull()
    {
        var save = SaveFileReader.Read(
            "{\"terraTokens\":0}@{\"planetId\":\"X\"}@@@@{\"craftedObjects\":0}@@@{\"version\":\"2.007\"}@");
        var pod = Resolver().ResolveSpawn(save, "X");
        Assert.Null(pod);
    }
}
