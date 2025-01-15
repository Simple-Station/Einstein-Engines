using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown.Mapping;
using Robust.Shared.Serialization.Markdown.Sequence;
using Robust.Shared.Serialization.Markdown.Validation;
using Robust.Shared.Serialization.Markdown.Value;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;
using Robust.Shared.Serialization.TypeSerializers.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.CompilerServices;

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

    [DataField("mobStateParams")]
    public Dictionary<string, string> InitMobStateParams = new()
    {
        {"Alive", "AliveDefault" },
        {"SoftCritical", "SoftCriticalDefault" },
        {"Critical", "CriticalDefault" },
        {"Dead", "DeadDefault" }
    };

    [AutoNetworkedField]
    public Dictionary<MobState, MobStateParametersPrototype> MobStateParams = new();

    /// <summary>
    /// Use this to modify mobstate parameters without actually overwrithing them.
    /// </summary>
    [AutoNetworkedField]
    public Dictionary<MobState, MobStateParametersOverride> MobStateParamsOverrides = new()
    {
        { MobState.Alive, new() },
        { MobState.SoftCritical, new() },
        { MobState.Critical, new() },
        { MobState.Dead, new() }
    };

    public MobStateParametersPrototype CurrentStateParams => MobStateParams[CurrentState];
    public MobStateParametersOverride CurrentStateOverrides => MobStateParamsOverrides[CurrentState];

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


    #region getters

    // the "?." and "?? false" at the end is because at round restard MobStateParams apparently can be wiped,
    // but stuff that relies on it will still run, and I don't know why. Cool.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanMove() => CurrentStateOverrides.Moving ?? CurrentStateParams?.Moving ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanTalk() => CurrentStateOverrides.Talking ?? CurrentStateParams?.Talking ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanEmote() => CurrentStateOverrides.Emoting ?? CurrentStateParams?.Emoting ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanThrow() => CurrentStateOverrides.Throwing ?? CurrentStateParams?.Throwing ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanPickUp() => CurrentStateOverrides.PickingUp ?? CurrentStateParams?.PickingUp ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanPull() => CurrentStateOverrides.Pulling ?? CurrentStateParams?.Pulling ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanAttack() => CurrentStateOverrides.Attacking ?? CurrentStateParams?.Attacking ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanUse() => CurrentStateOverrides.Using ?? CurrentStateParams?.Using ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanPoint() =>CurrentStateOverrides.Pointing ?? CurrentStateParams?.Pointing ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsConscious() => CurrentStateOverrides.IsConscious ?? CurrentStateParams?.IsConscious ?? false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsDowned() => CurrentStateOverrides.ForceDown ?? CurrentStateParams?.ForceDown ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsIncapacitated() => CurrentStateOverrides.Incapacitated ?? CurrentStateParams?.Incapacitated ?? false;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanBreathe() => CurrentStateOverrides.CanBreathe ?? CurrentStateParams?.CanBreathe ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanEquipSelf() => CurrentStateOverrides.CanEquipSelf ?? CurrentStateParams?.CanEquipSelf ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanUnequipSelf() => CurrentStateOverrides.CanUnequipSelf ?? CurrentStateParams?.CanUnequipSelf ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanEquipOther() => CurrentStateOverrides.CanEquipOther ?? CurrentStateParams?.CanEquipOther ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanUnequipOther() => CurrentStateOverrides.CanUnequipOther ?? CurrentStateParams?.CanUnequipOther ?? false;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float? GetOxyDamageOverlay() => CurrentStateOverrides.OxyDamageOverlay ?? CurrentStateParams?.OxyDamageOverlay;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetStrippingTimeMultiplier() => CurrentStateOverrides.StrippingTimeMultiplier ?? CurrentStateParams?.StrippingTimeMultiplier ?? 1f;
    #endregion
}

[NetSerializable, Serializable]
public struct MobStateParametersOverride
{
    [DataField]
    public bool? Moving, Talking, Emoting,
        Throwing, PickingUp, Pulling, Attacking, Using, Pointing,
        IsConscious,
        CanEquipSelf, CanUnequipSelf, CanEquipOther, CanUnequipOther,
        ForceDown, Incapacitated, CanBreathe;

    [DataField]
    public float? OxyDamageOverlay;

    [DataField]
    public float? StrippingTimeMultiplier;
}

[Prototype("mobStateParams")]
[Serializable] // do i need to put netserializable on it?
public sealed partial class MobStateParametersPrototype : IPrototype, IInheritingPrototype
{
    [IdDataField]
    public string ID { get; } = default!;

    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<MobStateParametersPrototype>))]
    public string[]? Parents { get; private set; }

    [AbstractDataField]
    [NeverPushInheritance]
    public bool Abstract { get; }

    [DataField]
    public bool? Moving, Talking, Emoting,
    Throwing, PickingUp, Pulling, Attacking, Using, Pointing,
    IsConscious,
    CanEquipSelf, CanUnequipSelf, CanEquipOther, CanUnequipOther,
    ForceDown, Incapacitated, CanBreathe = false;

    [DataField]
    public float? OxyDamageOverlay;

    [DataField]
    public float StrippingTimeMultiplier = 1f;
}
