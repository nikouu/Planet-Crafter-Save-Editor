using System.Text;
using PlanetCrafterSaveEditor.Models;

namespace PlanetCrafterSaveEditor.Parsing;

public static class SaveFileWriter
{
    public static byte[] Write(SaveFile save)
    {
        return Encoding.UTF8.GetBytes(WriteString(save));
    }

    public static string WriteString(SaveFile save)
    {
        if (save.Separators.Count != save.Records.Count + 1)
        {
            throw new InvalidOperationException(
                $"Separator count must be Records.Count + 1; got {save.Separators.Count} separators and {save.Records.Count} records.");
        }

        var sb = new StringBuilder();
        for (var i = 0; i < save.Records.Count; i++)
        {
            sb.Append(save.Separators[i]);
            sb.Append(save.Records[i].OriginalText);
        }
        sb.Append(save.Separators[^1]);
        return sb.ToString();
    }
}
