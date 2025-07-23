using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    /// Whether the RestrictedGear trait can be used on the server.
    /// </summary>
    public static readonly CVarDef<bool> RestrictedGearEnabled =
        CVarDef.Create("trait.restrictedgear_enabled", true, CVar.SERVERONLY);
}
