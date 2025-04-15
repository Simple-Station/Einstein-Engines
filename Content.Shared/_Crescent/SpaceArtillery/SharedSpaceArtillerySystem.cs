using Content.Shared.Actions;

namespace Content.Shared._Crescent.SpaceArtillery;

public sealed class SharedSpaceArtillerySystem : EntitySystem
{
}
/// <summary>
/// Raised when someone fires the artillery
/// </summary>
public sealed partial class FireActionEvent : InstantActionEvent
{
}
