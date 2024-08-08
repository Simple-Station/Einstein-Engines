using Robust.Shared.GameStates;

namespace Content.Shared.Traits.Assorted.Components;

/// <summary>
///   This is used for traits that make an entity immune to slips
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SlipImmunityComponent : Component
{
}
