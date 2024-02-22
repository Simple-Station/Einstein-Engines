using Content.Shared.Radio;
using Robust.Shared.Prototypes;

namespace Content.Server.Backmen.Blob;

[RegisterComponent]
public sealed partial class BlobSpeakComponent : Component
{
    public ProtoId<RadioChannelPrototype> Channel = "Hivemind";
}
