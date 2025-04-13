using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Shitmed.Medical.Surgery;

/// <summary>
///     Prevents the entity from screaming during surgery without having to be asleep.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class NoScreamComponent : Component { }
