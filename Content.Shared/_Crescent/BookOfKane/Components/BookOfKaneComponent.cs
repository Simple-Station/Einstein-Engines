using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Crescent.BookOfKane.Components;

[Serializable, NetSerializable]
public sealed partial class BookOfKaneDoAfterEvent : SimpleDoAfterEvent
{
}

[RegisterComponent, NetworkedComponent]
public sealed partial class BookOfKaneComponent : Component
{
}
