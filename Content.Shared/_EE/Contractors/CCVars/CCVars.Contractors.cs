using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

/// <summary>
/// Contains all the CVars used by Contractors.
/// </summary>
public sealed partial class CCVars
{
    /// <summary>
    ///     Will CharacterNationalityRequirements, CharacterEmployerRequirements and CharacterLifepathRequirements restrict other things?
    /// </summary>
    public static readonly CVarDef<bool> ContractorsCharacterRequirementsEnabled =
        CVarDef.Create("contractors.character_requirements", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    ///     Will CharacterNationalityRequirements, CharacterEmployerRequirements and CharacterLifepathRequirements execute their functions?
    /// </summary>
    public static readonly CVarDef<bool> ContractorsTraitFunctionsEnabled =
        CVarDef.Create("contractors.trait_functions", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    ///     Will a chosen Nationality spawn the player with a Passport?
    /// </summary>
    public static readonly CVarDef<bool> ContractorsPassportEnabled =
        CVarDef.Create("contractors.spawn_passports", true, CVar.SERVER | CVar.REPLICATED);

    /// <summary>
    ///     Master Switch for Contractors
    /// </summary>
    public static readonly CVarDef<bool> ContractorsEnabled =
        CVarDef.Create("contractors.enabled", true, CVar.SERVER | CVar.REPLICATED);
}
