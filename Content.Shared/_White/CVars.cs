using Robust.Shared.Configuration;

namespace Content.Shared._White;

[CVarDefs]
public sealed class WhiteCVars
{
    #region Keybind

    public static readonly CVarDef<bool> AutoGetUp =
        CVarDef.Create("white.auto_get_up", true, CVar.CLIENT | CVar.ARCHIVE | CVar.REPLICATED);

    public static readonly CVarDef<bool> HoldLookUp =
        CVarDef.Create("white.hold_look_up", false, CVar.CLIENT | CVar.ARCHIVE);

    #endregion
}
