using Content.Shared._White.UserInterface;
using Robust.Shared.Configuration;

namespace Content.Shared._White.CCVar;

public sealed partial class WhiteCVars
{
    public static readonly CVarDef<EmotesMenuType> EmotesMenuStyle =
        CVarDef.Create("interface.emotes_menu_style", EmotesMenuType.Window, CVar.CLIENT | CVar.ARCHIVE);
}
