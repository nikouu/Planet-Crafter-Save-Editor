using System.Text;

namespace PlanetCrafterSaveEditor.Services;

public sealed class GIdNamer
{
    private readonly IReadOnlyDictionary<string, string> _overrides;

    public GIdNamer(IReadOnlyDictionary<string, string> overrides)
    {
        _overrides = overrides;
    }

    public string Display(string gId)
    {
        if (_overrides.TryGetValue(gId, out var name))
        {
            return name;
        }
        return Prettify(gId);
    }

    public bool HasOverride(string gId) => _overrides.ContainsKey(gId);

    public IEnumerable<string> KnownGIds => _overrides.Keys;

    public static string Prettify(string gId)
    {
        if (string.IsNullOrEmpty(gId)) return gId;

        var sb = new StringBuilder(gId.Length + 8);
        var capitalizeNext = true;
        for (var i = 0; i < gId.Length; i++)
        {
            var c = gId[i];
            if (c == '-' || c == '_')
            {
                if (sb.Length > 0 && sb[^1] != ' ') sb.Append(' ');
                capitalizeNext = true;
                continue;
            }
            var prev = i > 0 ? gId[i - 1] : '\0';
            var insertSpace = i > 0 && (
                (char.IsUpper(c) && (char.IsLower(prev) || char.IsDigit(prev))) ||
                (char.IsDigit(c) && char.IsLetter(prev) && !char.IsDigit(prev)) ||
                (char.IsLetter(c) && char.IsDigit(prev)));
            if (insertSpace && sb.Length > 0 && sb[^1] != ' ')
            {
                sb.Append(' ');
            }
            sb.Append(capitalizeNext ? char.ToUpperInvariant(c) : c);
            capitalizeNext = false;
        }
        return sb.ToString();
    }
}
