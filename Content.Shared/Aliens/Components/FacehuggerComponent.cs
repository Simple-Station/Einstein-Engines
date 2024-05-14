using System.Threading;
using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;

namespace Content.Shared.Aliens.Components;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class FacehuggerComponent : Component
{
    [DataField]
    public float Range = 3f;

    [DataField]
    public bool Active = true;

    [ViewVariables]
    public TimeSpan GrowTime = TimeSpan.Zero;

    [DataField]
    public float EmbryoTime = 10f;

    [DataField]
    public ProtoId<PolymorphPrototype> FacehuggerPolymorphPrototype = "FacehuggerToInactive";

    public bool Equipped;

    public EntityUid Equipee;
}
