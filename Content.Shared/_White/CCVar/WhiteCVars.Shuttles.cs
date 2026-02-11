using Robust.Shared.Configuration;

namespace Content.Shared._White.CCVar;

public sealed partial class WhiteCVars
{
    /// <summary>
    /// WWDP - Can emergency shuttle's early launch authorizations be recalled.
    /// </summary>
    public static readonly CVarDef<bool> EmergencyAuthRecallAllowed =
        CVarDef.Create("shuttle.emergency_auth_recall_allowed", false, CVar.SERVERONLY);

}
