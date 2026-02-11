using Robust.Shared.Configuration;

namespace Content.Shared._White.CCVar;

public sealed partial class WhiteCVars
{
    /// <summary>
    /// Players to set their own clown names.
    /// </summary>
    public static readonly CVarDef<bool> AllowCustomClownName =
        CVarDef.Create("customize.allow_custom_clown_name", true, CVar.REPLICATED);

    /// <summary>
    /// Players to set their own mime names.
    /// </summary>
    public static readonly CVarDef<bool> AllowCustomMimeName =
        CVarDef.Create("customize.allow_custom_mime_name", true, CVar.REPLICATED);
}
