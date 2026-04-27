using PlanetCrafterSaveEditor.Models;
using PlanetCrafterSaveEditor.Parsing;

namespace PlanetCrafterSaveEditor.Services;

public sealed class SpawnResolver
{
    private readonly IReadOnlyDictionary<string, long> _shippedHashes;

    public SpawnResolver(IReadOnlyDictionary<string, long> shippedPlanetHashes)
    {
        _shippedHashes = shippedPlanetHashes;
    }

    public WorldObjectView? ResolveSpawn(SaveFile save, string planetId)
    {
        var pods = save.WorldObjects()
            .Where(w => w.GId.StartsWith("EscapePod", StringComparison.Ordinal))
            .ToList();
        if (pods.Count == 0) return null;

        // Step 1: suffixed match by gId
        var suffixed = pods.FirstOrDefault(w => w.GId == "EscapePod" + planetId);
        if (suffixed is not null) return suffixed;

        // Step 2: bare-EscapePod whose `planet` hash matches the dominant hash for WOs that have a position
        var dominant = save.WorldObjects()
            .Where(w => w.HasPosition && w.PlanetHash.HasValue)
            .GroupBy(w => w.PlanetHash!.Value)
            .OrderByDescending(g => g.Count())
            .FirstOrDefault();
        if (dominant is not null)
        {
            var bare = pods.FirstOrDefault(w => w.GId == "EscapePod" && w.PlanetHash == dominant.Key);
            if (bare is not null) return bare;
        }

        // Step 3: shipped fallback table — look up planetId, match WO with that planet hash
        if (_shippedHashes.TryGetValue(planetId, out var shippedHash))
        {
            var fallback = pods.FirstOrDefault(w => w.PlanetHash == shippedHash);
            if (fallback is not null) return fallback;
        }

        return null;
    }
}
