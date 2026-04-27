using PlanetCrafterSaveEditor.Parsing;

namespace PlanetCrafterSaveEditor.Tests;

public class RoundTripTests
{
    [SkippableTheory]
    [InlineData("PrimePlanetTest.json")]
    [InlineData("Prime2.json")]
    [InlineData("HumblePlanetTest.json")]
    [InlineData("SelenaPlanetTest.json")]
    [InlineData("ToxicityPlanetTest.json")]
    public void RoundTrip_ProducesByteIdenticalOutput(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "exampleSaves", fileName);
        Skip.IfNot(File.Exists(path), $"Example save not present: {path}");

        var bytes = File.ReadAllBytes(path);
        var save = SaveFileReader.Read(bytes);
        var roundTripped = SaveFileWriter.Write(save);

        Assert.Equal(bytes, roundTripped);
    }
}
