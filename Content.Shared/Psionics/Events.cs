using Robust.Shared.Serialization;
using Content.Shared.Damage;
using Content.Shared.DoAfter;

namespace Content.Shared.Psionics.Events;

[Serializable, NetSerializable]
public sealed partial class PsionicRegenerationDoAfterEvent : DoAfterEvent
{
    [DataField("startedAt", required: true)]
    public TimeSpan StartedAt;

    public PsionicRegenerationDoAfterEvent(TimeSpan startedAt)
    {
        StartedAt = startedAt;
    }

    public override DoAfterEvent Clone() => this;
}

[Serializable, NetSerializable]
public sealed partial class GlimmerWispDrainDoAfterEvent : SimpleDoAfterEvent { }

[Serializable, NetSerializable]
public sealed partial class HealingWordDoAfterEvent : DoAfterEvent
{
    [DataField(required: true)]
    public TimeSpan StartedAt;

    public HealingWordDoAfterEvent(TimeSpan startedAt)
    {
        StartedAt = startedAt;
    }

    public override DoAfterEvent Clone() => this;
}

[Serializable, NetSerializable]
public sealed partial class PsionicHealOtherDoAfterEvent : DoAfterEvent
{
    [DataField(required: true)]
    public TimeSpan StartedAt;

    [DataField]
    public DamageSpecifier HealingAmount = default!;

    [DataField]
    public float RotReduction;

    [DataField]
    public bool DoRevive;

    public PsionicHealOtherDoAfterEvent(TimeSpan startedAt)
    {
        StartedAt = startedAt;
    }

    public override DoAfterEvent Clone() => this;
}
