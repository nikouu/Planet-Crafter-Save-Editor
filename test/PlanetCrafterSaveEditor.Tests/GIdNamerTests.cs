using PlanetCrafterSaveEditor.Services;

namespace PlanetCrafterSaveEditor.Tests;

public class GIdNamerTests
{
    [Theory]
    [InlineData("ToxicGoo", "Toxic Goo")]
    [InlineData("astrofood", "Astrofood")]
    [InlineData("ScreenMap1", "Screen Map 1")]
    [InlineData("Rod-iridium", "Rod Iridium")]
    [InlineData("Minable-Tungsten", "Minable Tungsten")]
    [InlineData("Container2", "Container 2")]
    [InlineData("EscapePodToxicity", "Escape Pod Toxicity")]
    [InlineData("ice", "Ice")]
    public void Prettify_HandlesCamelCaseDigitsAndDashes(string input, string expected)
    {
        Assert.Equal(expected, GIdNamer.Prettify(input));
    }

    [Fact]
    public void Display_PrefersOverride_OverPrettify()
    {
        var namer = new GIdNamer(new Dictionary<string, string>
        {
            ["ToxicGoo"] = "Toxic Goo (curated)",
        });
        Assert.Equal("Toxic Goo (curated)", namer.Display("ToxicGoo"));
    }

    [Fact]
    public void Display_FallsBackToPrettify_WhenNoOverride()
    {
        var namer = new GIdNamer(new Dictionary<string, string>());
        Assert.Equal("Pristine Mushroom", namer.Display("PristineMushroom"));
    }
}
