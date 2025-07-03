using Content.Shared._NF.Shuttles.Events;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.BUIStates;

[Serializable, NetSerializable]
public sealed class NavInterfaceState
{
    public float MaxRange;

    /// <summary>
    /// The relevant coordinates to base the radar around.
    /// </summary>
    public NetCoordinates? Coordinates;

    /// <summary>
    /// The relevant rotation to rotate the angle around.
    /// </summary>
    public double Angle;

    public Dictionary<NetEntity, List<DockingPortState>> Docks;

    /// <summary>
    /// Frontier - the state of the shuttle's inertial dampeners
    /// </summary>
    public InertiaDampeningMode DampeningMode;

    /// <summary>
    /// Hullrot - target console for this UI state
    /// </summary>
    public NetEntity console;

    /// <summary>
    /// Hullrot - keep this aligned to world?
    /// </summary>
    public bool AlignToWorld = false;


    public NavInterfaceState(
        float maxRange,
        NetCoordinates? coordinates,
        double angle,
        Dictionary<NetEntity, List<DockingPortState>> docks,
        InertiaDampeningMode dampeningMode, // Frontier: add dampeningMode
        NetEntity Console)
    {
        MaxRange = maxRange;
        Coordinates = coordinates;
        Angle = angle;
        Docks = docks;
        DampeningMode = dampeningMode; // Frontier
        console = Console;
    }
}

[Serializable, NetSerializable]
public enum RadarConsoleUiKey : byte
{
    Key
}
 // hullrot added
[Serializable, NetSerializable]
public sealed class NavConsoleGroupPressedMessage(int payload) : BoundUserInterfaceMessage
{
    public int Payload = payload;
}
