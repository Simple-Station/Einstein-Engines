using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server.GameTicking.Rules.Components;

/// <summary>
/// Makes this rules antags spawn a humanoid, either from the player's profile or a random one.
/// </summary>
[RegisterComponent]
public sealed partial class AntagLoadProfileRuleComponent : Component
{
    /// <summary>
    /// If specified, the profile loaded will be made into this species if the chosen species matches the blacklist.
    /// </summary>
    [DataField]
    public ProtoId<SpeciesPrototype>? SpeciesOverride;

    /// <summary>
    /// List of species that trigger the override
    /// </summary>
    [DataField]
    public HashSet<ProtoId<SpeciesPrototype>>? SpeciesOverrideBlacklist;

    /// <summary>
    /// If true, then SpeciesOverride will always be used
    /// </summary>
    [DataField]
    public bool AlwaysUseSpeciesOverride;

    /// <summary>
    ///     Whether this antag will also attempt to generate Traits, Nationality, Employer, and Lifepath. Basically all the "Traits"
    /// </summary>
    [DataField]
    public bool AllowProfileExtensions = true;

    /// <summary>
    ///     Whether this antag will also generate Loadouts. Fun fact: Look up CharacterAntagonistRequirement
    /// </summary>
    [DataField]
    public bool AllowAntagLoadouts = true;
}
