using System.Globalization;

namespace PlanetCrafterSaveEditor.Models;

public readonly struct Vec3 : IEquatable<Vec3>
{
    public float X { get; }
    public float Y { get; }
    public float Z { get; }

    private readonly string? _original;

    public Vec3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
        _original = null;
    }

    private Vec3(float x, float y, float z, string original)
    {
        X = x;
        Y = y;
        Z = z;
        _original = original;
    }

    public static Vec3 Parse(string s)
    {
        if (TryParse(s, out var v))
        {
            return v;
        }
        throw new FormatException($"Invalid Vec3: '{s}'");
    }

    public static bool TryParse(string? s, out Vec3 vec)
    {
        vec = default;
        if (s is null)
        {
            return false;
        }
        var parts = s.Split(',');
        if (parts.Length != 3)
        {
            return false;
        }
        if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var x) ||
            !float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var y) ||
            !float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var z))
        {
            return false;
        }
        vec = new Vec3(x, y, z, s);
        return true;
    }

    public override string ToString()
    {
        if (_original is not null)
        {
            return _original;
        }
        var inv = CultureInfo.InvariantCulture;
        return string.Concat(
            X.ToString("G9", inv), ",",
            Y.ToString("G9", inv), ",",
            Z.ToString("G9", inv));
    }

    public bool Equals(Vec3 other) => X == other.X && Y == other.Y && Z == other.Z;
    public override bool Equals(object? obj) => obj is Vec3 v && Equals(v);
    public override int GetHashCode() => HashCode.Combine(X, Y, Z);
    public static bool operator ==(Vec3 a, Vec3 b) => a.Equals(b);
    public static bool operator !=(Vec3 a, Vec3 b) => !a.Equals(b);
}
