using PlanetCrafterSaveEditor.Models;
using PlanetCrafterSaveEditor.Parsing;
using PlanetCrafterSaveEditor.Services;

namespace PlanetCrafterSaveEditor.Tests;

public class TeleportTests
{
    private static (SaveSession session, byte[] originalBytes) Load(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "exampleSaves", fileName);
        Skip.IfNot(File.Exists(path), $"Example save not present: {path}");
        var bytes = File.ReadAllBytes(path);
        var session = new SaveSession();
        session.Load(bytes, fileName);
        return (session, bytes);
    }

    [SkippableFact]
    public void TeleportPlayer_ChangesOnlyPlayerPosition()
    {
        var (session, originalBytes) = Load("PrimePlanetTest.json");
        var player = session.Save!.Players().Single();
        var originalPos = player.PositionString;

        session.TeleportPlayer(player.Id, new Vec3(414.61f, 8.3f, 585.28f));

        var updatedPlayer = session.Save!.Players().Single();
        Assert.NotEqual(originalPos, updatedPlayer.PositionString);
        Assert.True(updatedPlayer.Record.IsDirty);

        Assert.Single(session.Save!.Records, r => r.IsDirty);
        Assert.True(session.IsDirty);

        var newBytes = session.Write();
        Assert.NotEqual(originalBytes, newBytes);

        var reparsed = SaveFileReader.Read(newBytes);
        var reparsedPlayer = reparsed.Players().Single();
        Assert.Equal(414.61f, reparsedPlayer.Position.X, 4);
        Assert.Equal(8.3f, reparsedPlayer.Position.Y, 4);
        Assert.Equal(585.28f, reparsedPlayer.Position.Z, 4);
    }

    [SkippableFact]
    public void TeleportPlayer_PreservesOtherPlayerFields()
    {
        var (session, _) = Load("ToxicityPlanetTest.json");
        var players = session.Save!.Players().ToList();
        var host = players.First(p => p.IsHost);
        var hostId = host.Id;
        var hostName = host.Name;
        var hostPlanet = host.PlanetId;

        // Snapshot the OriginalText of OTHER players
        var otherPlayerTexts = players.Where(p => p.Id != hostId)
            .Select(p => p.Record.OriginalText)
            .ToList();

        session.TeleportPlayer(hostId, new Vec3(100.5f, 200.25f, 300.125f));

        var refreshed = session.Save!.Players().ToList();
        var newHost = refreshed.First(p => p.Id == hostId);

        // Host name and planet preserved
        Assert.Equal(hostName, newHost.Name);
        Assert.Equal(hostPlanet, newHost.PlanetId);
        Assert.True(newHost.IsHost);

        // Other players' record texts unchanged byte-for-byte
        var newOtherTexts = refreshed.Where(p => p.Id != hostId)
            .Select(p => p.Record.OriginalText)
            .ToList();
        Assert.Equal(otherPlayerTexts, newOtherTexts);
    }

    [SkippableFact]
    public void TeleportWorldObject_ChangesOnlyPosField()
    {
        var (session, _) = Load("PrimePlanetTest.json");
        var pod = session.Save!.WorldObjects().Single();

        session.TeleportWorldObject(pod.Id, new Vec3(123.456f, 78.9f, -42.0f));

        Assert.Single(session.Save!.Records, r => r.IsDirty);
        var updated = session.Save.WorldObjects().Single();
        Assert.Equal(123.456f, updated.Position!.Value.X, 3);
        Assert.Equal(78.9f, updated.Position.Value.Y, 3);
        Assert.Equal(-42f, updated.Position.Value.Z, 3);

        // Other key fields preserved
        Assert.Equal("EscapePod", updated.GId);
        Assert.Equal(-1140328421L, updated.PlanetHash);
    }

    [SkippableFact]
    public void TeleportPlayer_UnknownId_Throws()
    {
        var (session, _) = Load("PrimePlanetTest.json");
        Assert.Throws<InvalidOperationException>(() =>
            session.TeleportPlayer(999, new Vec3(0, 0, 0)));
    }
}
