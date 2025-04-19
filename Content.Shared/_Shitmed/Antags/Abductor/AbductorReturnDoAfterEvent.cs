﻿using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._Shitmed.Antags.Abductor;

[Serializable, NetSerializable]
public sealed partial class AbductorReturnDoAfterEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class AbductorGizmoMarkDoAfterEvent : SimpleDoAfterEvent
{
}

[Serializable, NetSerializable]
public sealed partial class AbductorSendYourselfDoAfterEvent : SimpleDoAfterEvent
{
    [DataField("coordinates", required: true)]
    public NetCoordinates TargetCoordinates;

    private AbductorSendYourselfDoAfterEvent()
    {
    }

    public AbductorSendYourselfDoAfterEvent(NetCoordinates coords) => TargetCoordinates = coords;
    public override DoAfterEvent Clone() => this;
}
[Serializable, NetSerializable]
public sealed partial class AbductorAttractDoAfterEvent : SimpleDoAfterEvent
{
    [DataField("coordinates", required: true)]
    public NetCoordinates TargetCoordinates;

    [DataField("victim", required: true)]
    public NetEntity Victim;
    private AbductorAttractDoAfterEvent()
    {
    }

    public AbductorAttractDoAfterEvent(NetCoordinates coords, NetEntity target)
    {
        TargetCoordinates = coords;
        Victim = target;
    }

    public override DoAfterEvent Clone() => this;
}
