using Content.Shared.FixedPoint;
using Robust.Shared.Utility;
using Robust.Shared.Serialization;

namespace Content.Shared._Shitmed.Body.BodyCapacity;

/// <summary>
///     This class represents a collection of organ types and their capacities.
///     Yes. It's a shameless copy of DamageSpecifier for the sake of expandability.
/// </summary>
[DataDefinition, Serializable, NetSerializable]
public sealed partial class OrganCapacity
{
    /// <summary>
    ///     Main type and capacity dictionary.
    /// </summary>
    [DataField]
    public Dictionary<CapacityType, FixedPoint2> Types { get; set; } = new();

    /// <summary>
    ///     Returns a sum of the damage values.
    /// </summary>
    public FixedPoint2 GetTotalOfType(CapacityType capacityType)
    {
        var total = FixedPoint2.Zero;
        Types.TryGetValue(capacityType, out var value);
        return total;
    }

    #region constructors

    /// <summary>
    ///     Constructor that takes another OrganCapacity instance and copies it.
    /// </summary>
    public OrganCapacity(OrganCapacity organCapacity)
    {
        Types = new(organCapacity.Types);
    }

    #endregion constructors

    #region Operators
    public static OrganCapacity operator *(OrganCapacity organCapacity, FixedPoint2 factor)
    {
        OrganCapacity newOrganCapacity = new();
        foreach (var entry in organCapacity.Types)
        {
            newOrganCapacity.Types.Add(entry.Key, entry.Value * factor);
        }
        return newOrganCapacity;
    }

    public static OrganCapacity operator *(OrganCapacity organCapacity, (FixedPoint2 factor, CapacityType type) tuple)
    {
        OrganCapacity newOrganCapacity = new(organCapacity);
        if (newOrganCapacity.Types.ContainsKey(tuple.type))
        {
            newOrganCapacity.Types[tuple.type] *= tuple.factor;
        }
        return newOrganCapacity;
    }

    public static OrganCapacity operator *(OrganCapacity organCapacity, float factor)
    {
        return organCapacity * factor;
    }

    public static OrganCapacity operator *(OrganCapacity organCapacity, (float factor, CapacityType type) tuple)
    {
        return organCapacity * (tuple.factor, tuple.type);
    }

    // Division operators
    public static OrganCapacity operator /(OrganCapacity organCapacity, FixedPoint2 factor)
    {
        OrganCapacity newOrganCapacity = new();
        foreach (var entry in organCapacity.Types)
        {
            newOrganCapacity.Types.Add(entry.Key, entry.Value / factor);
        }
        return newOrganCapacity;
    }

    public static OrganCapacity operator /(OrganCapacity organCapacity, (FixedPoint2 factor, CapacityType type) tuple)
    {
        OrganCapacity newOrganCapacity = new(organCapacity);
        if (newOrganCapacity.Types.ContainsKey(tuple.type))
        {
            newOrganCapacity.Types[tuple.type] /= tuple.factor;
        }
        return newOrganCapacity;
    }

    public static OrganCapacity operator /(OrganCapacity organCapacity, float factor)
    {
        return organCapacity / factor;
    }

    public static OrganCapacity operator /(OrganCapacity organCapacity, (float factor, CapacityType type) tuple)
    {
        return organCapacity / (tuple.factor, tuple.type);
    }

    public static OrganCapacity operator +(OrganCapacity organCapacityA, OrganCapacity organCapacityB)
    {
        OrganCapacity newOrganCapacity = new(organCapacityA);

        foreach (var entry in organCapacityB.Types)
            if (!newOrganCapacity.Types.TryAdd(entry.Key, entry.Value))
                newOrganCapacity.Types[entry.Key] += entry.Value;

        return newOrganCapacity;
    }

    public static OrganCapacity operator +(OrganCapacity organCapacity, (FixedPoint2 value, CapacityType type) tuple)
    {
        OrganCapacity newOrganCapacity = new(organCapacity);
        if (newOrganCapacity.Types.ContainsKey(tuple.type))
            newOrganCapacity.Types[tuple.type] += tuple.value;

        return newOrganCapacity;
    }

    public static OrganCapacity operator -(OrganCapacity organCapacityA, OrganCapacity organCapacityB)
    {
        OrganCapacity newOrganCapacity = new(organCapacityA);

        foreach (var entry in organCapacityB.Types)
            if (!newOrganCapacity.Types.TryAdd(entry.Key, -entry.Value))
                newOrganCapacity.Types[entry.Key] -= entry.Value;

        return newOrganCapacity;
    }

    public static OrganCapacity operator -(OrganCapacity organCapacity, (FixedPoint2 value, CapacityType type) tuple)
    {
        OrganCapacity newOrganCapacity = new(organCapacity);
        if (newOrganCapacity.Types.ContainsKey(tuple.type))
            newOrganCapacity.Types[tuple.type] -= tuple.value;

        return newOrganCapacity;
    }

    public static OrganCapacity operator +(OrganCapacity organCapacity) => organCapacity;

    public static OrganCapacity operator -(OrganCapacity organCapacity) => organCapacity * -1;

    public static OrganCapacity operator *(float factor, OrganCapacity organCapacity) => organCapacity * factor;
    public static OrganCapacity operator *((float factor, CapacityType type) tuple, OrganCapacity organCapacity)
        => organCapacity * tuple;

    public static OrganCapacity operator *(FixedPoint2 factor, OrganCapacity organCapacity) => organCapacity * factor;
    public static OrganCapacity operator *((FixedPoint2 factor, CapacityType type) tuple, OrganCapacity organCapacity)
        => organCapacity * tuple;

    public FixedPoint2 this[CapacityType key] => Types[key];
    #endregion
}
