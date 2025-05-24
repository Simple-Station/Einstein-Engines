using Content.Shared.EventScheduler;

using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared.Emp;

/// <summary>
/// While entity has this component it is "disabled" by EMP.
/// Add desired behaviour in other systems
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
[Access(typeof(SharedEmpSystem))]
public sealed partial class EmpDisabledComponent : Component
{
    [DataField("effectCoolDown"), ViewVariables(VVAccess.ReadWrite)]
    public float EffectCooldown = 3f;

    /// <summary>
    /// When next effect will be spawned
    /// </summary>
    [AutoPausedField]
    public TimeSpan TargetTime = TimeSpan.Zero;

    /// <summary>
    /// Stores the last delayed event (EmpDisableRemoval) to extend its duration when stacking EMP
    /// </summary>
    [DataField]
    public DelayedEvent? LastDelayedEvent;
}
