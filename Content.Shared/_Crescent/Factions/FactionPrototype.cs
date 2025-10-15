using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;
using Robust.Shared.Utility;
using System.ComponentModel.DataAnnotations;

namespace Content.Shared.Roles;

[Prototype("faction")]
public sealed partial class FactionPrototype : IPrototype
{
    [IdDataField] public string ID { get; } = default!;

    [DataField("name", required: true)] public string Name = default!;

    /// <summary>
    ///     Used to add certain conditions to the faction like spawn restrictions. Text is red.
    /// </summary>
    [DataField("descriptionPrefix")] public string DescriptionPrefix = default!;

    /// <summary>
    ///     Used to set the color of the faction button. Default is dark gray.
    /// </summary>
    [DataField("buttonColor")] public Color FactionButtonColor = Color.DarkSlateGray;

    [DataField("description", required: true)] public string Description = default!;

    [DataField("icon", required: true)] public SpriteSpecifier Icon = SpriteSpecifier.Invalid;

    /// <summary>
    ///     A color representing this department to use for text.
    /// </summary>
    [DataField("color", required: true)]
    public Color Color = default!;

    /// <summary>
    /// Departments with a higher weight sorted before other departments in UI.
    /// </summary>
    [DataField("weight")]
    public int Weight { get; private set; } = 0;

    /// <summary>
    /// Frontier - whether or not to show this faction. Defaults to no.
    /// </summary>
    [DataField("enabled")]
    public bool Enabled = false;
}

/// <summary>
/// Sorts <see cref="FactionPrototype"/> appropriately for display in the UI,
/// respecting their <see cref="FactionPrototype.Weight"/>.
/// </summary>
public sealed class FactionUIComparer : IComparer<FactionPrototype>
{
    public static readonly FactionUIComparer Instance = new();

    public int Compare(FactionPrototype? x, FactionPrototype? y)
    {
        if (ReferenceEquals(x, y))
            return 0;
        if (ReferenceEquals(null, y))
            return 1;
        if (ReferenceEquals(null, x))
            return -1;

        var cmp = -x.Weight.CompareTo(y.Weight);
        if (cmp != 0)
            return cmp;
        return string.Compare(x.ID, y.ID, StringComparison.Ordinal);
    }
}
