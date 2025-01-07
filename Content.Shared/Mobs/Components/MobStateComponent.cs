using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared.Mobs.Components;

/// <summary>
///     When attached to an <see cref="DamageableComponent"/>,
///     this component will handle critical and death behaviors for mobs.
///     Additionally, it handles sending effects to clients
///     (such as blur effect for unconsciousness) and managing the health HUD.
/// </summary>
[RegisterComponent]
[NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class MobStateComponent : Component
{
    /// <summary>
    ///     Whether this mob will be allowed to issue movement commands when in the Critical MobState.
    /// </summary>
    [DataField]
    public bool AllowMovementWhileCrit;

    /// <summary>
    ///     Whether this mob will be allowed to issue movement commands when in the Soft-Crit MobState.
    /// </summary>
    [DataField]
    public bool AllowMovementWhileSoftCrit;

    /// <summary>
    ///     Whether this mob will be allowed to issue movement commands when in the Dead MobState.
    ///     This is provided for completeness sake, and *probably* shouldn't be used by default.
    /// </summary>
    [DataField]
    public bool AllowMovementWhileDead;

    /// <summary>
    ///     Whether this mob will be allowed to talk while in the Critical MobState.
    /// </summary>
    [DataField]
    public bool AllowTalkingWhileCrit = true;

    /// <summary>
    ///     Whether this mob will be allowed to talk while in the SoftCritical MobState.
    /// </summary>
    [DataField]
    public bool AllowTalkingWhileSoftCrit = true;

    /// <summary>
    ///     Whether this mob will be allowed to talk while in the Dead MobState.
    ///     This is provided for completeness sake, and *probably* shouldn't be used by default.
    /// </summary>
    [DataField]
    public bool AllowTalkingWhileDead;

    /// <summary>
    ///     Whether this mob is forced to be downed when entering the Critical MobState.
    /// </summary>
    [DataField]
    public bool DownWhenCrit = true;

    /// <summary>
    ///     Whether this mob is forced to be downed when entering the SoftCritical MobState.
    /// </summary>
    [DataField]
    public bool DownWhenSoftCrit = true;

    /// <summary>
    ///     Whether this mob is forced to be downed when entering the Dead MobState.
    /// </summary>
    [DataField]
    public bool DownWhenDead = true;

    /// <summary>
    ///     Whether this mob is allowed to perform hand interactions while in the Critical MobState.
    /// </summary>
    [DataField]
    public bool AllowHandInteractWhileCrit;

    /// <summary>
    ///     Whether this mob is allowed to perform hand interactions while in the SoftCritical MobState.
    /// </summary>
    [DataField]
    public bool AllowHandInteractWhileSoftCrit;

    /// <summary>
    ///     Whether this mob is allowed to perform hand interactions while in the Dead MobState.
    ///     This is provided for completeness sake, and *probably* shouldn't be used by default.
    /// </summary>
    [DataField]
    public bool AllowHandInteractWhileDead;

    //default mobstate is always the lowest state level
    [AutoNetworkedField, ViewVariables]
    public MobState CurrentState { get; set; } = MobState.Alive;

    [DataField]
    [AutoNetworkedField]
    public HashSet<MobState> AllowedStates = new()
        {
            MobState.Alive,
            MobState.Critical,
            MobState.SoftCritical,
            MobState.Dead
        };
}
