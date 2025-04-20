using Content.Shared.Crescent.Radar;
using Content.Shared.Shuttles.UI.MapObjects;
using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.BUIStates;

[Serializable, NetSerializable]
public sealed class ShuttleBoundUserInterfaceState : BoundUserInterfaceState
{
    public enum StateDirtyFlags : byte
    {
        None = 0,
        Base = 1,
        IFF = 2,
        All = Base | IFF,
    }

    public NavInterfaceState NavState;
    public ShuttleMapInterfaceState MapState;
    public DockingInterfaceState DockState;
    public CrewInterfaceState CrewState;

    public IFFInterfaceState IFFState;

    public StateDirtyFlags DirtyFlags;

    public bool canAccesCrew = false;

    public ShuttleBoundUserInterfaceState(NavInterfaceState navState, ShuttleMapInterfaceState mapState, DockingInterfaceState dockState, CrewInterfaceState crewState)
    {
        NavState = navState;
        MapState = mapState;
        DockState = dockState;
        CrewState = crewState;
        IFFState = default!;
        DirtyFlags = StateDirtyFlags.Base;
    }

    public ShuttleBoundUserInterfaceState(ShuttleBoundUserInterfaceState other)
    {
        NavState = other.NavState;
        MapState = other.MapState;
        DockState = other.DockState;
        CrewState = other.CrewState;
        IFFState = other.IFFState;
        DirtyFlags = other.DirtyFlags;
        canAccesCrew = other.canAccesCrew;
    }
}
