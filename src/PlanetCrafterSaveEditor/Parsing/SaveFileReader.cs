using System.Text;
using PlanetCrafterSaveEditor.Models;

namespace PlanetCrafterSaveEditor.Parsing;

public static class SaveFileReader
{
    public static SaveFile Read(byte[] bytes)
    {
        var text = Encoding.UTF8.GetString(bytes);
        return Read(text);
    }

    public static SaveFile Read(string text)
    {
        var records = new List<SaveRecord>();
        var separators = new List<string>();

        var separator = new StringBuilder();
        var i = 0;
        while (i < text.Length)
        {
            var c = text[i];
            if (c == '{')
            {
                separators.Add(separator.ToString());
                separator.Clear();

                var recordEnd = FindRecordEnd(text, i);
                records.Add(new SaveRecord { OriginalText = text.Substring(i, recordEnd - i) });
                i = recordEnd;
            }
            else
            {
                separator.Append(c);
                i++;
            }
        }
        separators.Add(separator.ToString());

        return new SaveFile { Records = records, Separators = separators };
    }

    private static int FindRecordEnd(string text, int start)
    {
        var depth = 0;
        var inString = false;
        var escape = false;

        for (var i = start; i < text.Length; i++)
        {
            var c = text[i];

            if (escape)
            {
                escape = false;
                continue;
            }

            if (inString)
            {
                if (c == '\\')
                {
                    escape = true;
                }
                else if (c == '"')
                {
                    inString = false;
                }
                continue;
            }

            switch (c)
            {
                case '"':
                    inString = true;
                    break;
                case '{':
                    depth++;
                    break;
                case '}':
                    depth--;
                    if (depth == 0)
                    {
                        return i + 1;
                    }
                    break;
            }
        }

        throw new FormatException("Unterminated JSON object in save file.");
    }
}
