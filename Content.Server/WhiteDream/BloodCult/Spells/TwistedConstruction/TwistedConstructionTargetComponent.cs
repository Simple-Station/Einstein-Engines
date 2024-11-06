using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.Spells.TwistedConstruction;

[RegisterComponent]
public sealed partial class TwistedConstructionTargetComponent : Component
{
    [DataField(required: true)]
    public EntProtoId ReplacementProto = "";

    [DataField]
    public TimeSpan DoAfterDelay = TimeSpan.FromSeconds(2);
}
