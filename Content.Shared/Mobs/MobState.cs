using Content.Shared.Mobs.Components;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;
using System.Runtime.CompilerServices;

namespace Content.Shared.Mobs;

/// <summary>
///     Defines what state an <see cref="Robust.Shared.GameObjects.EntityUid"/> is in.
///     Ordered from most alive to least alive.
/// </summary>
[Serializable, NetSerializable]
public enum MobState : byte
{
    Invalid = 0,
    Alive = 1,
    SoftCritical = 2,
    Critical = 3,
    Dead = 4
}

/// <summary>
/// Event that is raised whenever a MobState changes on an entity
/// </summary>
/// <param name="Target">The Entity whose MobState is changing</param>
/// <param name="Component">The MobState Component owned by the Target entity</param>
/// <param name="OldMobState">The previous MobState</param>
/// <param name="NewMobState">The new MobState</param>
/// <param name="Origin">The Entity that caused this state change</param>
public record struct MobStateChangedEvent(EntityUid Target, MobStateComponent Component, MobState OldMobState,
    MobState NewMobState, EntityUid? Origin = null);



/// <summary>
/// so i've decided to implement softcrit and now i have to go through
/// all the mobstate comparisons and make sure stuff that uses MobState.Critical
/// knows about MobState.SoftCritical.
///
/// A better approach to this would probably be look like "IsConscious", "IsDying" etc.
/// which would be more generalised.
///
/// But for now i just want to wrap all MobState comparisons in these static methods so it's easier to deal with.
/// </summary>
public static class MobStateHelpers
{
    //^.^ // fuck off

    /// <summary>
    /// Returns true if newmobstate is crit/softcrit and the old one isn't.
    /// if ignoreDead is true, will not return true if mobstate changed from dead to crit/softcrit.
    /// </summary>
    public static bool EnteredCrit(this MobStateChangedEvent ev, bool ignoreDead = true)
    {
        if (ignoreDead)
            return ev.IsCrit() && ev.WasCritOrAlive();
        return ev.IsCrit() && (ev.WasCrit() || ev.WasAlive());
    }
    /// <summary>
    /// Returns true if newmobstate is crit/softcrit and the old one isn't.
    /// if ignoreDead is true, will not return true if mobstate changed from crit/softcrit to dead.
    /// </summary>
    public static bool ExitedCrit(this MobStateChangedEvent ev, bool ignoreDead = true)
    {
        if (ignoreDead)
            return ev.WasCrit() && ev.IsCritOrAlive();
        return ev.WasCrit() && (ev.IsCrit() || ev.IsAlive());
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCritOrDead(this MobStateChangedEvent ev, bool CountSoftCrit = true) => ev.NewMobState.IsCritOrDead(CountSoftCrit);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCritOrAlive(this MobStateChangedEvent ev, bool CountSoftCrit = true) => ev.NewMobState.IsCritOrAlive(CountSoftCrit);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCrit(this MobStateChangedEvent ev, bool CountSoftCrit = true) => ev.NewMobState.IsCrit(CountSoftCrit);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDead(this MobStateChangedEvent ev) => ev.NewMobState.IsDead();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAlive(this MobStateChangedEvent ev) => ev.NewMobState.IsAlive();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValid(this MobStateChangedEvent ev) => ev.NewMobState.IsValid();

    public static bool WasCritOrDead(this MobStateChangedEvent ev, bool CountSoftCrit = true) => ev.OldMobState.IsCritOrDead(CountSoftCrit);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool WasCritOrAlive(this MobStateChangedEvent ev, bool CountSoftCrit = true) => ev.OldMobState.IsCritOrAlive(CountSoftCrit);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool WasCrit(this MobStateChangedEvent ev, bool CountSoftCrit = true) => ev.OldMobState.IsCrit(CountSoftCrit);
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool WasDead(this MobStateChangedEvent ev) => ev.OldMobState.IsDead();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool WasAlive(this MobStateChangedEvent ev) => ev.OldMobState.IsAlive();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool WasValid(this MobStateChangedEvent ev) => ev.OldMobState.IsValid();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCritOrDead(this MobState state, bool CountSoftCrit = true) => state.IsCrit(CountSoftCrit) || state.IsDead();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsCritOrAlive(this MobState state, bool CountSoftCrit = true) => state.IsCrit(CountSoftCrit) || state.IsDead();
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsDead(this MobState state) => state == MobState.Dead;
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAlive(this MobState state) => state == MobState.Alive;
    /// <summary>
    /// i think i am finally losing it. I do not remember writhing this and
    /// i cannot fathom what situation would this be warranted in.
    ///

    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsAliveOrDead(this MobState state, bool CountSoftCrit = true) => state.IsDead() || state.IsAlive();

    public static bool IsCrit(this MobState state, bool CountSoftCrit = true)
    {
        switch (state)
        {
            case MobState.SoftCritical:
                return CountSoftCrit;
            case MobState.Critical:
                return true;
            default:
                return false;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool IsValid(this MobState state) // fix your shit. mobstate should never be invalid.
    {
        DebugTools.Assert(state != MobState.Invalid, "MobState is invalid.");
        return state != MobState.Invalid;
    }
}

//This is dumb and I hate it but I don't feel like refactoring this garbage
[Serializable, NetSerializable]
public enum MobStateVisuals : byte
{
    State
}
