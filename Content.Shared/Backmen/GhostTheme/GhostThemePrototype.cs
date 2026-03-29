using Robust.Shared.Prototypes;

namespace Content.Shared.Backmen.GhostTheme;

[Prototype("ghostTheme", -2)]
public sealed partial class GhostThemePrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; } = default!;

    [DataField]
    public LocId Name;

    [DataField("components")]
    [AlwaysPushInheritance]
    public ComponentRegistry Components { get; } = new();
}
