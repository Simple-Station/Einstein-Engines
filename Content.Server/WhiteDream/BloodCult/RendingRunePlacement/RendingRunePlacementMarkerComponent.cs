using Robust.Shared.GameStates;

namespace Content.Server.WhiteDream.BloodCult.RendingRunePlacement;

[RegisterComponent, NetworkedComponent]
public sealed partial class RendingRunePlacementMarkerComponent : Component
{
    [DataField]
    public float DrawingRange = 10;
}
