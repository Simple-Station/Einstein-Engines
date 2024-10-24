using Content.Shared.Traits.Assorted.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Traits.Assorted.Components;

/// <summary>
/// Set player speed to zero and standing state to down, simulating leg paralysis.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(LegsParalyzedSystem))]
public sealed partial class LegsParalyzedComponent : Component
{
}
