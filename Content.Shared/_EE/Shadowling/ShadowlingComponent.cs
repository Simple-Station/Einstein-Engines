using Robust.Shared.GameStates;

namespace Content.Shared._EE.Shadowling;


[RegisterComponent, NetworkedComponent, Access(typeof(ShadowlingSystem))]
public sealed partial class ShadowlingComponent : Component
{
}
