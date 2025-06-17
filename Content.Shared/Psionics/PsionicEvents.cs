using Content.Shared.Abilities.Psionics;
using Robust.Shared.Serialization;

namespace Content.Shared.Psionics;

/// <summary>
///     This event is raised whenever a psionic entity sets their casting stats(Amplification and Dampening), allowing other systems to modify the end result
///     of casting stat math. Useful if for example you want a species to have 5% higher Amplification overall. Or a drug inhibits total Dampening, etc.
/// </summary>
[ByRefEvent]
public record struct OnSetPsionicStatsEvent(float AmplificationChangedAmount, float DampeningChangedAmount);

[ByRefEvent]
public record struct OnMindbreakEvent();

/// <summary>
///     This event is raised at the end of all component initializations, and represents the entrypoint to initializing a psionic caster.
///     At this step, systems are invited to make any desired modifications to the initial power pool. No system is permitted to
///     actually handle power generation at this step.
/// </summary>
[ByRefEvent]
public record struct PsionicInitEvent(PsionicComponent PsionicComponent);

/// <summary>
///     During this step, the available powers list for a psionic caster should be fully generated. Subscribe to this for any system that wishes to generate powers for a caster.
/// </summary>
[ByRefEvent]
public record struct PsiPowersInitEvent(PsionicComponent PsionicComponent);

public sealed class PsionicPowerUsedEvent : HandledEntityEventArgs
{
    public EntityUid User { get; }
    public string Power;

    public PsionicPowerUsedEvent(EntityUid user, string power)
    {
        User = user;
        Power = power;
    }
}

public sealed class OnAttemptPowerUseEvent : CancellableEntityEventArgs
{
    public EntityUid User { get; }
    public string Power = string.Empty;

    public OnAttemptPowerUseEvent(EntityUid user, string power)
    {
        User = user;
        Power = power;
    }
}

[Serializable]
[NetSerializable]
public sealed class PsionicsChangedEvent : EntityEventArgs
{
    public readonly NetEntity Euid;
    public PsionicsChangedEvent(NetEntity euid)
    {
        Euid = euid;
    }
}
