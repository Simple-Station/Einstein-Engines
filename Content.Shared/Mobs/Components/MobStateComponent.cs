using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

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

    [DataField]
    public Dictionary<MobState, MobStateParameters> MobStateParams = new()
    {
        { MobState.Alive, new() },
        { MobState.SoftCritical, new() },
        { MobState.Critical, new() },
        { MobState.Dead, new() }
    };

    public MobStateParameters CurrentStateParams => MobStateParams[CurrentState];

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
                MobState.SoftCritical,
                MobState.Critical,
                MobState.Dead
            };



    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanMove() => CurrentStateParams.Overrides.Moving ?? CurrentStateParams.Moving;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanTalk() => CurrentStateParams.Overrides.Talking ?? CurrentStateParams.Talking;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanEmote() => CurrentStateParams.Overrides.Emoting ?? CurrentStateParams.Emoting;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanThrow() => CurrentStateParams.Overrides.Throwing ?? CurrentStateParams.Throwing;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanPickUp() => CurrentStateParams.Overrides.PickingUp ?? CurrentStateParams.PickingUp;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanPull() => CurrentStateParams.Overrides.Pulling ?? CurrentStateParams.Pulling;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanAttack() => CurrentStateParams.Overrides.Attacking ?? CurrentStateParams.Attacking;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanUse() => CurrentStateParams.Overrides.Using ?? CurrentStateParams.Using;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanPoint() =>CurrentStateParams.Overrides.Pointing ?? CurrentStateParams.Pointing;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ConsciousAttemptAllowed() => CurrentStateParams.Overrides.ConsciousAttemptsAllowed ?? CurrentStateParams.ConsciousAttemptsAllowed;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsDowned() => CurrentStateParams.Overrides.ForceDown ?? CurrentStateParams.ForceDown;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanEquipSelf() => CurrentStateParams.Overrides.CanEquipSelf ?? CurrentStateParams.CanEquipSelf;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanUnequipSelf() => CurrentStateParams.Overrides.CanUnequipSelf ?? CurrentStateParams.CanUnequipSelf;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanEquipOther() => CurrentStateParams.Overrides.CanEquipOther ?? CurrentStateParams.CanEquipOther;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanUnequipOther() => CurrentStateParams.Overrides.CanUnequipOther ?? CurrentStateParams.CanUnequipOther;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float? GetOxyDamageOverlay() => CurrentStateParams.Overrides.OxyDamageOverlay ?? CurrentStateParams.OxyDamageOverlay;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetStrippingTimeMultiplier() => CurrentStateParams.Overrides.StrippingTimeMultiplier ?? CurrentStateParams.StrippingTimeMultiplier;
}

[DataDefinition]
[NetSerializable, Serializable]
public sealed partial class MobStateParameters
{
    public MobStateParameters() { }

    [DataField]
    public bool Moving, Talking, Emoting,
        Throwing, PickingUp, Pulling, Attacking, Using, Pointing;

    [DataField]
    public bool ForceDown;

    [DataField]
    public bool ConsciousAttemptsAllowed;

    [DataField]
    public float? OxyDamageOverlay;

    [DataField]
    public bool CanEquipSelf, CanUnequipSelf, CanEquipOther, CanUnequipOther;

    [DataField]
    public float StrippingTimeMultiplier = 1f;

    [DataField]
    public MobStateParametersOverride Overrides = new();
}

[NetSerializable, Serializable]
public struct MobStateParametersOverride
{
    [DataField]
    public bool? Moving, Talking, Emoting,
        Throwing, PickingUp, Pulling, Attacking, Using, Pointing;

    [DataField]
    public bool? ForceDown;

    [DataField]
    public bool? ConsciousAttemptsAllowed;

    [DataField]
    public float? OxyDamageOverlay;

    [DataField]
    public float? StrippingTimeMultiplier;

    [DataField]
    public bool? CanEquipSelf, CanUnequipSelf, CanEquipOther, CanUnequipOther;
}

//[Prototype("mobStateParams")]
//public sealed partial class MobStateParamsPrototype : IPrototype, IInheritingPrototype
//{
//
//}
