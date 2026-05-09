using PlanetCrafterSaveEditor.Models;
using PlanetCrafterSaveEditor.Parsing;
using PlanetCrafterSaveEditor.Services;

namespace PlanetCrafterSaveEditor.Tests;

public class DiscardAllChangesTests
{
    private static SaveSession Load(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "exampleSaves", fileName);
        Skip.IfNot(File.Exists(path), $"Example save not present: {path}");
        var bytes = File.ReadAllBytes(path);
        var session = new SaveSession();
        session.Load(bytes, fileName);
        return session;
    }

    [Fact]
    public void DiscardAllChanges_ThrowsWhenNoSaveLoaded()
    {
        var session = new SaveSession();
        Assert.Throws<InvalidOperationException>(() => session.DiscardAllChanges());
    }

    [SkippableFact]
    public void DiscardAllChanges_ClearsDirtyEdits()
    {
        var session = Load("PrimePlanetTest.json");
        var player = session.Save!.Players().First();
        session.TeleportPlayer(player.Id, new Vec3(1f, 2f, 3f));
        Assert.True(session.IsDirty);

        session.DiscardAllChanges();

        Assert.False(session.IsDirty);
        Assert.DoesNotContain(session.Save!.Records, r => r.IsDirty);
    }

    [SkippableFact]
    public void DiscardAllChanges_RestoresOriginalValues()
    {
        var session = Load("PrimePlanetTest.json");
        var player = session.Save!.Players().First();
        var originalPos = player.PositionString;
        session.TeleportPlayer(player.Id, new Vec3(99f, 99f, 99f));
        Assert.NotEqual(originalPos, session.Save!.Players().First().PositionString);

        session.DiscardAllChanges();

        Assert.Equal(originalPos, session.Save!.Players().First().PositionString);
    }

    [SkippableFact]
    public void DiscardAllChanges_RaisesOnChanged()
    {
        var session = Load("PrimePlanetTest.json");
        var fired = 0;
        session.OnChanged += () => fired++;

        session.DiscardAllChanges();

        Assert.Equal(1, fired);
    }

    [SkippableFact]
    public void CanDiscard_FalseWhenClean_TrueWhenDirty()
    {
        var session = Load("PrimePlanetTest.json");
        Assert.False(session.CanDiscard);

        var player = session.Save!.Players().First();
        session.TeleportPlayer(player.Id, new Vec3(1f, 2f, 3f));

        Assert.True(session.CanDiscard);
    }
}
