using Content.Shared.Customization.Systems;
using Content.Shared.Psionics;
using Robust.Shared.Prototypes;

namespace Content.Shared.Traits;


/// <summary>
///     Describes a trait.
/// </summary>
[Prototype("trait")]
public sealed partial class TraitPrototype : IPrototype
{
    [ViewVariables]
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    ///     Which customization tab to place this entry in
    /// </summary>
    [DataField(required: true), ValidatePrototypeId<TraitCategoryPrototype>]
    public string Category = "Uncategorized";

    /// <summary>
    ///     How many points this will give the character
    /// </summary>
    [DataField]
    public int Points = 0;


    [DataField]
    public List<CharacterRequirement> Requirements = new();

    /// <summary>
    ///     The components that get added to the player when they pick this trait.
    /// </summary>
    [DataField]
    public ComponentRegistry? Components { get; private set; } = default!;

    /// <summary>
    ///     The list of each Action that this trait adds in the form of ActionId and ActionEntity
    /// </summary>
    [DataField]
    public List<EntProtoId>? Actions { get; private set; } = default!;

    /// <summary>
    ///     The list of all Psionic Powers that this trait adds. If this list is not empty, the trait will also Ensure that a player is Psionic.
    /// </summary>
    [DataField]
    public List<PsionicPowerPrototype>? PsionicPowers { get; private set; } = default!;
}
