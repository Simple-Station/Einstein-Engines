using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    public static readonly CVarDef<bool> CanOperateOnSelf =
        CVarDef.Create("surgery.can_operate_on_self", false, CVar.SERVERONLY);
}
