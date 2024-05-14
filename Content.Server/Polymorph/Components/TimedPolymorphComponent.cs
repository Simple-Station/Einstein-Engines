using System.Threading;
using Content.Shared.Polymorph;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server.Polymorph.Components;

/// <summary>
/// This is used for polymorphing entity after time
/// </summary>
[RegisterComponent]
public sealed partial class TimedPolymorphComponent : Component
{
    [DataField(required: true)]
    public ProtoId<PolymorphPrototype> PolymorphPrototype;

    [DataField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Magic/forcewall.ogg");

    [DataField]
    public float PolymorphTime = 5f;

    [DataField]
    public bool Enabled = true;

    public CancellationTokenSource? TokenSource;
}
