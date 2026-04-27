using PlanetCrafterSaveEditor.Models;

namespace PlanetCrafterSaveEditor.Services;

public enum WorldObjectCategory
{
    Container,
    Vehicle,
    DeathChest,
    Drop,
    Other,
}

public sealed class WorldObjectCategorizer
{
    private readonly IReadOnlyDictionary<WorldObjectCategory, IReadOnlyList<string>> _patterns;

    public WorldObjectCategorizer(IReadOnlyDictionary<WorldObjectCategory, IReadOnlyList<string>> patterns)
    {
        _patterns = patterns;
    }

    public static WorldObjectCategorizer Default() => new(new Dictionary<WorldObjectCategory, IReadOnlyList<string>>
    {
        [WorldObjectCategory.DeathChest] = new[] { "DeathChest" },
        [WorldObjectCategory.Container] = new[] { "Chest", "Locker", "Container", "StorageBox" },
        [WorldObjectCategory.Vehicle] = new[] { "Rover", "Vehicle", "Drone" },
    });

    public WorldObjectCategory Categorize(WorldObjectView wo)
    {
        // Match-priority order: DeathChest beats Container (DeathChest contains "Chest")
        foreach (var category in new[] { WorldObjectCategory.DeathChest, WorldObjectCategory.Container, WorldObjectCategory.Vehicle })
        {
            if (!_patterns.TryGetValue(category, out var patterns)) continue;
            foreach (var pat in patterns)
            {
                if (wo.GId.Contains(pat, StringComparison.Ordinal))
                {
                    return category;
                }
            }
        }
        return wo.HasPosition ? WorldObjectCategory.Drop : WorldObjectCategory.Other;
    }
}
