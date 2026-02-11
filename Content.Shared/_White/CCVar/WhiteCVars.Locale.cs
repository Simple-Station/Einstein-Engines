using Robust.Shared.Configuration;

namespace Content.Shared._White.CCVar;

public sealed partial class WhiteCVars
{
    public static readonly CVarDef<string>
        ServerCulture = CVarDef.Create("locale.culture", "ru-RU", CVar.REPLICATED | CVar.SERVER);
}
