using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared.Clothing.Loadouts.Prototypes;
using Content.Shared.Roles;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Preferences.Loadouts;

[Serializable, NetSerializable]
public sealed class LoadoutPreferences
{
    public readonly List<CharacterLoadout> Loadouts;

    /// <summary>
    ///     Index of the selected loadout in <see cref="Loadouts"/>. Negative if no loadout is selected.
    /// </summary>
    public int SelectedLoadout { get; init; }

    /// <summary>
    ///     Mapping of job prototype IDs to their preferred loadout indices.
    /// </summary>
    /// <remarks>
    ///     As this stores positional references to items of <see cref="Loadouts"/>, this dictionary must be updated when removing indices from that list.
    /// </remarks>
    public Dictionary<string, int> JobPreferences { get; init; }

    public LoadoutPreferences(List<CharacterLoadout> loadouts, int selectedLoadout, Dictionary<string, int> jobPreferences)
    {
        Loadouts = loadouts;
        SelectedLoadout = selectedLoadout;
        JobPreferences = jobPreferences;
    }

    public LoadoutPreferences() : this(new(), -1, new()) {}

    public LoadoutPreferences(LoadoutPreferences other) : this(other.Loadouts, other.SelectedLoadout, other.JobPreferences) {}

    public LoadoutPreferences Clone() => new(this);

    /// <summary>
    ///     Attempts to retrieve a loadout profile with the specified idx, or the current selected loadout profile if none is provided.
    /// </summary>
    public bool TryGetLoadout([NotNullWhen(true)] out CharacterLoadout? loadout, int index = -1)
    {
        if (index < 0)
            index = SelectedLoadout;

        if (index < 0 || index >= Loadouts.Count)
        {
            loadout = null;
            return false;
        }

        loadout = Loadouts[index];
        return true;
    }

    /// <summary>
    ///     Checks if the current loadout profile has the specified loadout selected.
    /// </summary>
    public bool CurrentLoadoutContains(ProtoId<LoadoutPrototype> loadout)
    {
        return TryGetLoadout(out var loadoutData) && loadoutData.Items.Contains(loadout);
    }

    /// <summary>
    ///     Returns a copy of these preferences with all invalid (non-existent) loadout items and job preferences removed.
    /// </summary>
    public LoadoutPreferences EnsureValid(IPrototypeManager protoman)
    {
        var copy = Clone();
        foreach (var loadout in copy.Loadouts)
        {
            var items = loadout.Items;
            for (var i = 0; i < items.Count; i++)
            {
                if (!protoman.HasIndex(items[i]))
                    items.RemoveAt(i--);
            }
        }

        foreach (var preference in JobPreferences)
        {
            if (!copy.JobPreferences.ContainsKey(preference.Key)
                || preference.Value >= 0 && preference.Value < copy.Loadouts.Count && protoman.HasIndex<JobPrototype>(preference.Key))
                continue;

            copy.JobPreferences.Remove(preference.Key);
        }

        return copy;
    }

    /// <summary>
    ///     Returns this object with a specific loadout profile removed. This method updates the job preferences accordingly.
    /// </summary>
    public LoadoutPreferences WithoutLoadout(int idx)
    {
        if (idx < 0 || idx > Loadouts.Count)
            throw new ArgumentException("Loadout idx is invalid.");

        return new LoadoutPreferences(this)
        {
            JobPreferences =
                JobPreferences.Where(it => it.Value != idx)
                .Select(it => (it.Key, it.Value > idx ? it.Value - 1 : it.Value))
                .ToDictionary()
        };
    }

    private bool Equals(LoadoutPreferences other)
    {
        return Loadouts.SequenceEqual(other.Loadouts)
               && SelectedLoadout == other.SelectedLoadout
               && JobPreferences.SequenceEqual(other.JobPreferences);
    }

    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is LoadoutPreferences other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Loadouts, SelectedLoadout, JobPreferences);
    }
}
