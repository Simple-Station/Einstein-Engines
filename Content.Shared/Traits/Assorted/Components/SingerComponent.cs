using Content.Shared.Traits.Assorted.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Traits.Assorted.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SingerComponent : Component
{
    // Traits are server-only, and is this is added via traits, it must be replicated to the client.
    [DataField(required: true), AutoNetworkedField]
    public ProtoId<SingerInstrumentPrototype> Proto = string.Empty;

    [DataField(serverOnly: true)]
    public EntProtoId? MidiActionId = "ActionHarpyPlayMidi";

    [DataField(serverOnly: true)]
    public EntityUid? MidiAction;
}
