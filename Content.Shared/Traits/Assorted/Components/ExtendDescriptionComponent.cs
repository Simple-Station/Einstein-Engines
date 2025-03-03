using Content.Shared.Customization.Systems;
using Robust.Shared.Serialization;

namespace Content.Shared.Traits.Assorted.Components;

[Serializable, NetSerializable, DataDefinition]
public sealed partial class DescriptionExtension
{
    [DataField]
    public List<CharacterRequirement> Requirements = new();

    [DataField]
    public string Description = "";

    [DataField]
    public string RequirementsNotMetDescription = "";

    [DataField]
    public int FontSize = 12;

    [DataField]
    public string Color = "#ffffff";

    /// todo: replace this with a CharacterRequirement; this will need updating a bunch of yml before this field can be safely removed.
    /// Deprecated.
    [DataField]
    public bool RequireDetailRange = true;
}

[RegisterComponent]
public sealed partial class ExtendDescriptionComponent : Component
{
    /// <summary>
    ///     The list of all descriptions to add to an entity when examined at close range.
    /// </summary>
    [DataField]
    public List<DescriptionExtension> DescriptionList = new();
}
