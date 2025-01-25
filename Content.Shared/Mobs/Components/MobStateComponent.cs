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

    [AutoNetworkedField, ViewVariables]
    public Dictionary<MobState, MobStateParametersPrototype> MobStateParams = new();

    /// <summary>
    /// Use this to modify mobstate parameters without actually overwrithing them.
    /// </summary>
    [AutoNetworkedField, ViewVariables]
    public Dictionary<MobState, MobStateParametersOverride> MobStateParamsOverrides = new()
    {
        { MobState.Alive, new() },
        { MobState.SoftCritical, new() },
        { MobState.Critical, new() },
        { MobState.Dead, new() }
    };
    [ViewVariables]
    public MobStateParametersPrototype CurrentStateParams => MobStateParams[CurrentState];
    [ViewVariables]
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


    #region terraria wall of getters boss

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private MobStateParametersOverride GetOverride(MobState? State = null) { MobStateParamsOverrides.TryGetValue(State ?? CurrentState, out var value); return value; }
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private MobStateParametersPrototype? GetParams(MobState? State = null) { MobStateParams.TryGetValue(State ?? CurrentState, out var value); return value; }


    // the "?." and "?? false" at the end is because at round restard MobStateParams apparently can be wiped,
    // but stuff that relies on it will still run, and I don't know why. Cool.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanMove(MobState? State = null) => GetOverride(State).Moving ?? GetParams(State)?.Moving ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanTalk(MobState? State = null) => GetOverride(State).Talking ?? GetParams(State)?.Talking ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanEmote(MobState? State = null) => GetOverride(State).Emoting ?? GetParams(State)?.Emoting ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanThrow(MobState? State = null) => GetOverride(State).Throwing ?? GetParams(State)?.Throwing ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanPickUp(MobState? State = null) => GetOverride(State).PickingUp ?? GetParams(State)?.PickingUp ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanPull(MobState? State = null) => GetOverride(State).Pulling ?? GetParams(State)?.Pulling ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanAttack(MobState? State = null) => GetOverride(State).Attacking ?? GetParams(State)?.Attacking ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanUse(MobState? State = null) => GetOverride(State).Using ?? GetParams(State)?.Using ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanPoint(MobState? State = null) =>GetOverride(State).Pointing ?? GetParams(State)?.Pointing ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsConscious(MobState? State = null) => GetOverride(State).IsConscious ?? GetParams(State)?.IsConscious ?? false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsDowned(MobState? State = null) => GetOverride(State).ForceDown ?? GetParams(State)?.ForceDown ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ShouldDropItems(MobState? State = null) => GetOverride(State).DropItemsOnEntering ?? GetParams(State)?.DropItemsOnEntering ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsIncapacitated(MobState? State = null) => GetOverride(State).Incapacitated ?? GetParams(State)?.Incapacitated ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool IsThreatening(MobState? State = null) => GetOverride(State).Threatening ?? GetParams(State)?.Threatening ?? true; // :)
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanEquipSelf(MobState? State = null) => GetOverride(State).CanEquipSelf ?? GetParams(State)?.CanEquipSelf ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanUnequipSelf(MobState? State = null) => GetOverride(State).CanUnequipSelf ?? GetParams(State)?.CanUnequipSelf ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanEquipOther(MobState? State = null) => GetOverride(State).CanEquipOther ?? GetParams(State)?.CanEquipOther ?? false;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool CanUnequipOther(MobState? State = null) => GetOverride(State).CanUnequipOther ?? GetParams(State)?.CanUnequipOther ?? false;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float? GetOxyDamageOverlay(MobState? State = null) => GetOverride(State).OxyDamageOverlay ?? GetParams(State)?.OxyDamageOverlay;
    /// <summary>
    /// Not clamped, but you really should, in case someone decides to be funny with the prototypes.
    /// [0,1].
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetBreathingMultiplier(MobState? State = null) => GetOverride(State).BreathingMultiplier ?? GetParams(State)?.BreathingMultiplier ?? 1f;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public float GetStrippingTimeMultiplier(MobState? State = null) => GetOverride(State).StrippingTimeMultiplier ?? GetParams(State)?.StrippingTimeMultiplier ?? 1f;
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
        ForceDown, DropItemsOnEntering, Incapacitated, Threatening;

    [DataField]
    public float? OxyDamageOverlay, BreathingMultiplier, StrippingTimeMultiplier;
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
    ForceDown, DropItemsOnEntering, Incapacitated, Threatening;

    [DataField]
    public float? OxyDamageOverlay, BreathingMultiplier, StrippingTimeMultiplier;
}


public static class MobStateParametersPrototypeExt
{
    public static void MergeWith(this MobStateParametersPrototype p, MobStateParametersPrototype other)
    {
        p.Moving = other.Moving ?? p.Moving;
        p.Talking = other.Talking ?? p.Talking;
        p.Emoting = other.Emoting ?? p.Emoting;
        p.Throwing = other.Throwing ?? p.Throwing;
        p.PickingUp = other.PickingUp ?? p.PickingUp;
        p.Pulling = other.Pulling ?? p.Pulling;
        p.Attacking = other.Attacking ?? p.Attacking;
        p.Using = other.Using ?? p.Using;
        p.Pointing = other.Pointing ?? p.Pointing;
        p.IsConscious = other.IsConscious ?? p.IsConscious;
        p.CanEquipSelf = other.CanEquipSelf ?? p.CanEquipSelf;
        p.CanEquipOther = other.CanEquipOther ?? p.CanEquipOther;
        p.CanUnequipSelf = other.CanUnequipSelf ?? p.CanUnequipSelf;
        p.CanUnequipOther = other.CanUnequipOther ?? p.CanUnequipOther;
        p.ForceDown = other.ForceDown ?? p.ForceDown;
        p.Incapacitated = other.Incapacitated ?? p.Incapacitated;
        p.Threatening = other.Threatening ?? p.Threatening;
        p.DropItemsOnEntering = other.DropItemsOnEntering ?? p.DropItemsOnEntering;
        p.BreathingMultiplier = other.BreathingMultiplier ?? p.BreathingMultiplier;
        p.OxyDamageOverlay = other.OxyDamageOverlay ?? p.OxyDamageOverlay;
        p.StrippingTimeMultiplier = other.StrippingTimeMultiplier ?? p.StrippingTimeMultiplier;
    }
}
