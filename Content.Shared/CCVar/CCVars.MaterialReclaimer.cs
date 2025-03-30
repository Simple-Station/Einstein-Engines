using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Whether or not a Material Reclaimer is allowed to eat people when emagged.
    /// </summary>
    public static readonly CVarDef<bool> ReclaimerAllowGibbing =
        CVarDef.Create("reclaimer.allow_gibbing", true, CVar.SERVER);
}
