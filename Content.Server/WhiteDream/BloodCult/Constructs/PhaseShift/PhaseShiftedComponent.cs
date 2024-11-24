﻿using Content.Shared.Physics;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.WhiteDream.BloodCult.Constructs.PhaseShift;

[RegisterComponent]
public sealed partial class PhaseShiftedComponent : Component
{
    [DataField]
    public string StatusEffectId = "PhaseShifted";

    [DataField]
    public float MovementSpeedBuff = 1.5f;

    [DataField]
    public int CollisionMask = (int) CollisionGroup.GhostImpassable;

    [DataField]
    public int CollisionLayer;

    [DataField]
    public EntProtoId PhaseInEffect = "EffectEmpPulseNoSound";

    [DataField]
    public EntProtoId PhaseOutEffect = "EffectEmpPulseNoSound";

    [DataField]
    public SoundSpecifier PhaseInSound = new SoundPathSpecifier(new ResPath("/Audio/WhiteDream/BloodCult/veilin.ogg"));

    [DataField]
    public SoundSpecifier PhaseOutSound =
        new SoundPathSpecifier(new ResPath("/Audio/WhiteDream/BloodCult/veilout.ogg"));

    public int StoredMask;
    public int StoredLayer;
}
