using Robust.Shared.Configuration;

namespace Content.Shared._White.CCVar;

public sealed partial class WhiteCVars
{
    public static readonly CVarDef<bool> CombatModeSoundEnabled =
        CVarDef.Create("combatMode.toggle_sound", false, CVar.CLIENTONLY | CVar.ARCHIVE);
}
