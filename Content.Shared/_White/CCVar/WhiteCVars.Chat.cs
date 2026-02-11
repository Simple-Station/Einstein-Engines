using Robust.Shared.Configuration;

namespace Content.Shared._White.CCVar;

public sealed partial class WhiteCVars
{
    public static readonly CVarDef<bool> LogInChat =
        CVarDef.Create("chat.log", true, CVar.CLIENT | CVar.ARCHIVE | CVar.REPLICATED);

    public static readonly CVarDef<bool> ChatFancyFont =
        CVarDef.Create("chat.fancy_font", true, CVar.CLIENTONLY | CVar.ARCHIVE);

    public static readonly CVarDef<bool> ColoredBubbleChat =
        CVarDef.Create("chat.colored_bubble", true, CVar.CLIENTONLY | CVar.ARCHIVE);
}
