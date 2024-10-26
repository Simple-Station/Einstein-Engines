using System.Linq;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Preferences.Loadouts;

[Serializable, NetSerializable]
public sealed class CharacterLoadout
{
    public readonly string Name;
    public readonly List<string> Items;

    public CharacterLoadout(string name, List<string> items)
    {
        Name = name;
        Items = items;
    }

    public CharacterLoadout Clone() => new(Name, new(Items));

    private bool Equals(CharacterLoadout other)
    {
        return Name == other.Name && Items.SequenceEqual(other.Items);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is CharacterLoadout other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Items);
    }
}
