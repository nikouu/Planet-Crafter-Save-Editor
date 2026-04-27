namespace PlanetCrafterSaveEditor.Models;

public sealed class SaveFile
{
    public IList<SaveRecord> Records { get; init; } = new List<SaveRecord>();

    public IList<string> Separators { get; init; } = new List<string>();
}

public sealed class SaveRecord
{
    public required string OriginalText { get; set; }

    public bool IsDirty { get; set; }
}
