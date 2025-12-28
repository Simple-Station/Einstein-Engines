using Robust.Shared.Configuration;

namespace Content.Shared._White.CCVar;

public sealed partial class WhiteCVars
{
    public static readonly CVarDef<bool> ToggleLookUp =
        CVarDef.Create("eye.toggle_look_up", false, CVar.CLIENT | CVar.ARCHIVE);
}
