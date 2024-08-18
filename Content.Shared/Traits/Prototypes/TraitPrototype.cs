using Content.Shared.Customization.Systems;
using Content.Shared.Prototypes;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Shared.Traits;


/// <summary>
///     Describes a trait.
/// </summary>
[Prototype("trait")]
public sealed class TraitPrototype : LocalizedPrototype
{
    /// <summary>The localization string for the description of this trait</summary>
    public string DescriptionLoc => ToLocalizationString("desc");
    /// <summary>The localized string for the description of this trait</summary>
    public string Description => Loc.GetString(DescriptionLoc);

    /// <summary>
    ///     Which customization tab to place this entry in
    /// </summary>
    [DataField(required: true), ValidatePrototypeId<TraitCategoryPrototype>]
    public string Category = "Uncategorized";

    /// <summary>
    ///     How many points this will give the character
    /// </summary>
    [DataField]
    public int Points;


    [DataField]
    public List<CharacterRequirement> Requirements = new();

    /// <summary>
    ///     The components that get added to the player when they pick this trait.
    /// </summary>
    [DataField(required: true)]
    public ComponentRegistry Components { get; private set; } = default!;
}
