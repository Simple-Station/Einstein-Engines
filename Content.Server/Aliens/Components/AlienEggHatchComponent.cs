using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;

namespace Content.Server.Aliens.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class AlienEggHatchComponent : Component
{
    [DataField(required: true)]
    public ProtoId<PolymorphPrototype> PolymorphPrototype;

    [DataField]
    public float ActivationRange = 1f;
}
