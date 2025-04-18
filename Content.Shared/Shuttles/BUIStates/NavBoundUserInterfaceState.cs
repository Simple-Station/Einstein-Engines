using Content.Shared.Crescent.Radar;
using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.BUIStates;

/// <summary>
/// Wrapper around <see cref="NavInterfaceState"/>
/// </summary>
[Serializable, NetSerializable]
public sealed class NavBoundUserInterfaceState : BoundUserInterfaceState
{

    // hullrot edited
    public enum StateDirtyFlags : byte
    {
        None = 0,
        Base = 1,
        IFF = 2,
        All = Base | IFF,
    }

    public NavInterfaceState State;

    public IFFInterfaceState IFFState;

    public StateDirtyFlags DirtyFlags;

    public NavBoundUserInterfaceState(NavInterfaceState state)
    {
        State = state;
        IFFState = default!;
        DirtyFlags = StateDirtyFlags.Base;
    }

    public NavBoundUserInterfaceState(NavBoundUserInterfaceState other)
    {
        State = other.State;
        IFFState = other.IFFState;
        DirtyFlags = other.DirtyFlags;
    }
}
