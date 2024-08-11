using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.InteractionVerbs.Events;

[Serializable, NetSerializable]
public sealed partial class InteractionVerbDoAfterEvent : SimpleDoAfterEvent
{
    [DataField]
    public InteractionVerbPrototype VerbPrototype;

    public InteractionVerbDoAfterEvent(InteractionVerbPrototype verbPrototype)
    {
        VerbPrototype = verbPrototype;
    }
}
