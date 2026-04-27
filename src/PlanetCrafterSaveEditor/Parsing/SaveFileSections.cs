using PlanetCrafterSaveEditor.Models;

namespace PlanetCrafterSaveEditor.Parsing;

public static class SaveFileSections
{
    public static IEnumerable<(int Index, SaveSection Section, SaveRecord Record)> EnumerateBySection(this SaveFile save)
    {
        var section = 0;
        for (var i = 0; i < save.Records.Count; i++)
        {
            section += CountAtSeparators(save.Separators[i]);
            yield return (i, (SaveSection)section, save.Records[i]);
        }
    }

    public static IEnumerable<SaveRecord> RecordsIn(this SaveFile save, SaveSection section)
    {
        foreach (var (_, s, record) in save.EnumerateBySection())
        {
            if (s == section)
            {
                yield return record;
            }
        }
    }

    public static IEnumerable<PlayerView> Players(this SaveFile save)
        => save.RecordsIn(SaveSection.Players).Select(r => new PlayerView(r));

    public static IEnumerable<WorldObjectView> WorldObjects(this SaveFile save)
        => save.RecordsIn(SaveSection.WorldObjects).Select(r => new WorldObjectView(r));

    public static IEnumerable<InventoryView> Inventories(this SaveFile save)
        => save.RecordsIn(SaveSection.Inventories).Select(r => new InventoryView(r));

    public static SaveMetaView? SaveMeta(this SaveFile save)
    {
        var record = save.RecordsIn(SaveSection.SaveMeta).FirstOrDefault();
        return record is null ? null : new SaveMetaView(record);
    }

    private static int CountAtSeparators(string separator)
    {
        var count = 0;
        foreach (var c in separator)
        {
            if (c == '@')
            {
                count++;
            }
        }
        return count;
    }
}
