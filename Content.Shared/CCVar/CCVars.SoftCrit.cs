using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     Used for basic Soft-Crit implementation. Entities are allowed to crawl when in crit, as this CVar intercepts the mover controller check for incapacitation,
    ///     and prevents it from stopping movement if this CVar is set to true and the user is Crit but Not Dead. This is only for movement,
    ///     you still can't stand up while crit, and you're still more or less helpless.
    /// </summary>
    public static readonly CVarDef<bool> AllowMovementWhileCrit =
        CVarDef.Create("mobstate.allow_movement_while_crit", true, CVar.REPLICATED);

    public static readonly CVarDef<bool> AllowTalkingWhileCrit =
        CVarDef.Create("mobstate.allow_talking_while_crit", true, CVar.REPLICATED);

    /// <summary>
    ///     Currently does nothing because I would have to figure out WHERE I would even put this check, and the mover controller is fairly complicated.
    ///     The goal is to make it so that attempting to move while in 'soft crit' can potentially cause further injury, causing you to die faster. Ideally there would be special
    ///     actions that can be performed in soft crit, such as applying pressure to your own injuries to slow down the bleedout, or other varieties of "Will To Live".
    /// </summary>
    public static readonly CVarDef<bool> DamageWhileCritMove =
        CVarDef.Create("mobstate.damage_while_crit_move", false, CVar.REPLICATED);
}
