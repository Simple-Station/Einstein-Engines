using Content.Shared.DoAfter;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.InteractionVerbs.Events;

[Serializable, NetSerializable]
public sealed partial class InteractionVerbDoAfterEvent : SimpleDoAfterEvent
{
    [DataField]
    public ProtoId<InteractionVerbPrototype> VerbPrototype;

    public InteractionVerbDoAfterEvent(ProtoId<InteractionVerbPrototype> verbPrototype)
    {
        VerbPrototype = verbPrototype;
    }
}
