using Content.Shared.ActionBlocker;
using Content.Shared.Administration.Logs;
using Content.Shared.Mobs.Components;
using Content.Shared.Standing;
using Robust.Shared.GameStates;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Shared.Mobs.Systems;

[Virtual]
public partial class MobStateSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly ILogManager _logManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        _sawmill = _logManager.GetSawmill("MobState");
        base.Initialize();
        SubscribeEvents();
    }

    #region Public API

    /// <summary>
    ///  Check if a Mob is Alive
    /// </summary>
    /// <param name="target">Target Entity</param>
    /// <param name="component">The MobState component owned by the target</param>
    /// <returns>If the entity is alive</returns>
    public bool IsAlive(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;
        return component.CurrentState == MobState.Alive;
    }

    /// <summary>
    ///  Check if a Mob is Critical
    /// </summary>
    /// <param name="target">Target Entity</param>
    /// <param name="component">The MobState component owned by the target</param>
    /// <returns>If the entity is Critical</returns>
    public bool IsCritical(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;
        return component.CurrentState.IsCrit();
    }

    public bool IsCriticalOrDead(EntityUid target, MobStateComponent? component = null) => IsCriticalOrDead(target, true, component);
    public bool IsCriticalOrDead(EntityUid target, bool countSoftCrit = true, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;
        return component.CurrentState.IsCritOrDead(countSoftCrit);
    }

    public bool IsHardCritical(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;
        return component.CurrentState == MobState.Critical;
    }

    public bool IsSoftCritical(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;
        return component.CurrentState == MobState.SoftCritical;
    }

    /// <summary>
    ///  Check if a Mob is Dead
    /// </summary>
    /// <param name="target">Target Entity</param>
    /// <param name="component">The MobState component owned by the target</param>
    /// <returns>If the entity is Dead</returns>
    public bool IsDead(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;

        return component.CurrentState == MobState.Dead;
    }




    /// <summary>
    ///  Check if a Mob is in an Invalid state
    /// </summary>
    /// <param name="target">Target Entity</param>
    /// <param name="component">The MobState component owned by the target</param>
    /// <returns>If the entity is in an Invalid State</returns>
    public bool IsInvalidState(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, true)) // i think this one should log it
            return false;
        return component.CurrentState.IsValid();
    }


    public bool CanMove(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;
        return component.CanMove();
    }

    public bool CanSpeak(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;
        return component.CanTalk();
    }

    public bool CanEmote(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;
        return component.CanEmote();
    }

    public bool CanThrow(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;
        return component.CanThrow();
    }

    public bool CanPickUp(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;
        return component.CanPickUp();
    }

    public bool CanPull(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;
        return component.CanPull();
    }

    public bool CanAttack(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;
        return component.CanAttack();
    }

    public bool CanUse(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;
        return component.CanUse();
    }

    public bool CanPoint(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;
        return component.CanPoint();
    }

    public bool ConsciousAttemptAllowed(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;
        return component.IsConscious();
    }

    public bool IsDown(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;
        return component.IsDowned();
    }

    public bool IsConscious(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;
        return component.IsConscious();
    }


    /// <summary>
    ///  Check if a Mob is incapacitated in its current MobState.
    /// </summary>
    /// <param name="target">Target Entity</param>
    /// <param name="component">The MobState component owned by the target</param>
    /// <returns>If the entity is Critical or Dead</returns>
    public bool IsIncapacitated(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;
        return component.IsIncapacitated();
    }

    public bool CanEquipSelf(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;
        return component.CanEquipSelf();
    }

    public bool CanUnequipSelf(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;
        return component.CanUnequipSelf();
    }

    public bool CanEquipOther(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;
        return component.CanEquipOther();
    }

    public bool CanUnequipOther(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;
        return component.CanUnequipOther();
    }



    public bool IsThreatening(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return false;
        return component.IsThreatening();
    }
    /// <summary>
    /// Clamped to [0,1]. Use <see cref="MobStateComponent.GetBreathingMultiplier"/> to get unclamped value.
    /// </summary>
    public float BreatheMultiplier(EntityUid target, MobStateComponent? component = null)
    {
        if (!Resolve(target, ref component, false))
            return 0f;
        return Math.Clamp(component.GetBreathingMultiplier(), 0, 1);
    }


    #endregion

    #region Private Implementation

    #endregion
}
