using Robust.Shared.GameStates;

namespace Content.Shared.Movement.Components;

/// <summary>
/// Indicates if the entity can toggle walking or not.
/// </summary>
[NetworkedComponent, RegisterComponent]
public sealed partial class CanWalkComponent : Component
{
}
