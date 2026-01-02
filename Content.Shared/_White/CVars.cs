using Robust.Shared.Configuration;

namespace Content.Shared._White;

[CVarDefs]
public sealed class WhiteCVars
{
    #region OptionsMisc

    public static readonly CVarDef<bool> LogInChat =
        CVarDef.Create("white.log_in_chat", true, CVar.CLIENT | CVar.ARCHIVE | CVar.REPLICATED);

    #endregion
}
