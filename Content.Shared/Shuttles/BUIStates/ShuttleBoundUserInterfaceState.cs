using Content.Shared.Crescent.Radar;
using Content.Shared.Shuttles.UI.MapObjects;
using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.BUIStates;
[Flags][Serializable][NetSerializable]
public enum StateDirtyFlags : byte
{
    None = 0,
    Base = 1,
    IFF = 2,
    Dock = 4,
    All = Base | IFF | Dock,
}

[Serializable, NetSerializable]
public sealed class ShuttleBoundUserInterfaceState : BoundUserInterfaceState
{

    public NavInterfaceState? NavState;
    public ShuttleMapInterfaceState? MapState;
    public DockingInterfaceState? DockState;
    public CrewInterfaceState? CrewState;

    public IFFInterfaceState? IFFState;

    public StateDirtyFlags DirtyFlags = StateDirtyFlags.None;

    public bool canAccesCrew = false;

    // YOu might ask why . Its because _ui.setUi is tick-based instead of event based. As such... we  need this cause the old state gets overridden SPCR 2025 . The
    // proper fix would be just splitting the UI,s but the fucking Nav UIs are a mess (and just adding another UIkey doesnt fucking work >:( )
    public int sendingDock = 0;

    public ShuttleBoundUserInterfaceState(NavInterfaceState navState, ShuttleMapInterfaceState mapState, DockingInterfaceState dockState, CrewInterfaceState crewState)
    {
        NavState = navState;
        MapState = mapState;
        DockState = dockState;
        CrewState = crewState;
        DirtyFlags = StateDirtyFlags.Base;
    }
    // empty constructor
    public ShuttleBoundUserInterfaceState()
    {
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
