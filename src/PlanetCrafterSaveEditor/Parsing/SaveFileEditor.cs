using PlanetCrafterSaveEditor.Models;

namespace PlanetCrafterSaveEditor.Parsing;

internal static class SaveFileEditor
{
    public static void RemoveRecordAt(SaveFile save, int index)
    {
        var merged = save.Separators[index] + save.Separators[index + 1];
        save.Separators[index] = merged;
        save.Separators.RemoveAt(index + 1);
        save.Records.RemoveAt(index);
    }

    public static int AppendToSection(SaveFile save, SaveSection section, SaveRecord newRecord)
    {
        var sectionIndices = new List<int>();
        var iter = save.EnumerateBySection().ToList();
        foreach (var (idx, sec, _) in iter)
        {
            if (sec == section)
            {
                sectionIndices.Add(idx);
            }
        }

        var withinSep = InferWithinSectionSeparator(save, section, iter);

        if (sectionIndices.Count == 0)
        {
            // Empty section. Find the position where section starts (separator with '@' counts).
            // The new record goes at the start of where this section begins.
            // Search separators left-to-right, accumulating section index.
            var sectionAcc = 0;
            for (var i = 0; i <= save.Records.Count; i++)
            {
                sectionAcc += CountAt(save.Separators[i]);
                if (sectionAcc == (int)section)
                {
                    // Insert new record at index i. Separator-before becomes withinSep (NOT containing '@').
                    // But the existing separator at i carries the '@' that brought us to this section.
                    // We need: existing_sep_with_@ + newRecord + withinSep + ... rest.
                    // So splice: existing separator stays at i, insert new sep at i+1 (which is withinSep without '@'),
                    // and insert record at i.
                    save.Records.Insert(i, newRecord);
                    save.Separators.Insert(i + 1, withinSep);
                    return i;
                }
                if (i < save.Records.Count) continue;
            }
            throw new InvalidOperationException($"Section {section} not found in save.");
        }

        var lastIdx = sectionIndices[^1];
        // Insert after lastIdx as a new record. Pre-separator = withinSep.
        save.Records.Insert(lastIdx + 1, newRecord);
        save.Separators.Insert(lastIdx + 1, withinSep);
        return lastIdx + 1;
    }

    private static string InferWithinSectionSeparator(SaveFile save, SaveSection section, List<(int Index, SaveSection Section, SaveRecord Record)> iter)
    {
        // A within-section separator is the separator just before a record whose section equals the previous record's section.
        for (var k = 1; k < iter.Count; k++)
        {
            if (iter[k].Section == section && iter[k - 1].Section == section)
            {
                return save.Separators[iter[k].Index];
            }
        }
        // Fall back: detect multi-line style by checking any existing separator
        var multiLine = save.Separators.Any(s => s.Contains('\n'));
        return multiLine ? "|\n" : "|";
    }

    private static int CountAt(string separator)
    {
        var count = 0;
        foreach (var c in separator)
        {
            if (c == '@') count++;
        }
        return count;
    }
}
