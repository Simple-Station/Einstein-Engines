using Robust.Shared.Prototypes;

namespace Content.Server._White.GameTicking.Aspects.Components;

[RegisterComponent]
public sealed partial class SpawnAspectComponent : Component
{
    [DataField(required: true)]
    public EntProtoId Prototype;

    [DataField]
    public int Min = 150;

    [DataField]
    public int Max = 200;
}
