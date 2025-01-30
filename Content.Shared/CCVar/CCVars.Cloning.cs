using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     How much should the cost to clone an entity be multiplied by.
    /// </summary>
    public static readonly CVarDef<float> CloningBiomassCostMultiplier =
        CVarDef.Create("cloning.biomass_cost_multiplier", 1f, CVar.SERVERONLY);

    /// <summary>
    ///     Whether or not the Biomass Reclaimer is allowed to roundremove bodies with a soul.
    /// </summary>
    public static readonly CVarDef<bool> CloningReclaimSouledBodies =
        CVarDef.Create("cloning.reclaim_souled_bodies", true, CVar.SERVERONLY);

    /// <summary>
    ///     Controls whether or not Metempsychosis will potentially give people a sex change.
    /// </summary>
    public static readonly CVarDef<bool> CloningPreserveSex =
        CVarDef.Create("cloning.preserve_sex", false, CVar.SERVERONLY);

    /// <summary>
    ///     Controls whether or not Metempsychosis preserves Pronouns when reincarnating people.
    /// </summary>
    public static readonly CVarDef<bool> CloningPreserveGender =
        CVarDef.Create("cloning.preserve_gender", true, CVar.SERVERONLY);

    /// <summary>
    ///     Controls whether or not Metempsychosis preserves Age.
    /// </summary>
    public static readonly CVarDef<bool> CloningPreserveAge =
        CVarDef.Create("cloning.preserve_age", false, CVar.SERVERONLY);

    /// <summary>
    ///     Controls whether or not Metempsychosis preserves height.
    /// </summary>
    public static readonly CVarDef<bool> CloningPreserveHeight =
        CVarDef.Create("cloning.preserve_height", false, CVar.SERVERONLY);

    /// <summary>
    ///     Controls whether or not Metempsychosis preserves width.
    /// </summary>
    public static readonly CVarDef<bool> CloningPreserveWidth =
        CVarDef.Create("cloning.preserve_width", false, CVar.SERVERONLY);

    /// <summary>
    ///     Controls whether or not Metempsychosis preserves Names. EG: Are you actually a new person?
    /// </summary>
    public static readonly CVarDef<bool> CloningPreserveName =
        CVarDef.Create("cloning.preserve_name", true, CVar.SERVERONLY);

    /// <summary>
    ///     Controls whether or not Metempsychosis preserves Flavor Text.
    /// </summary>
    public static readonly CVarDef<bool> CloningPreserveFlavorText =
        CVarDef.Create("cloning.preserve_flavor_text", true, CVar.SERVERONLY);
}
