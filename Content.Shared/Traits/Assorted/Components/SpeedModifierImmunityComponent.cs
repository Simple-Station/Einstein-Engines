using Robust.Shared.GameStates;

namespace Content.Shared.Traits.Assorted.Components;

/// <summary>
///   This is used to make an entity's movement speed constant and
///   never affected by almost all movement speed modifiers.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpeedModifierImmunityComponent : Component
{
}
