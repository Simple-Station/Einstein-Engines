using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.TimedFactory;

[RegisterComponent]
public sealed partial class TimedFactoryComponent : Component
{
    [DataField(required: true)]
    public List<EntProtoId> Prototypes = new();

    [DataField]
    public float Cooldown = 240;

    [ViewVariables(VVAccess.ReadOnly)]
    public float CooldownRemaining = 0;
}
