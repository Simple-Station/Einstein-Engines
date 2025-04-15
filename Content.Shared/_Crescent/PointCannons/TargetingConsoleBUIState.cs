using System.Numerics;
using Content.Shared.Crescent.Radar;
using Content.Shared.Shuttles.BUIStates;
using Robust.Shared.Serialization;

namespace Content.Shared.PointCannons;

[Serializable, NetSerializable]
public sealed class TargetingConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    public NavInterfaceState NavState;
    public IFFInterfaceState IFFState;
    public List<string>? CannonGroups;
    public List<NetEntity>? ControlledCannons;

    public TargetingConsoleBoundUserInterfaceState(
        NavInterfaceState navState,
        IFFInterfaceState iffState,
        List<string>? groups,
        List<NetEntity>? controlled)
    {
        NavState = navState;
        IFFState = iffState;
        CannonGroups = groups;
        ControlledCannons = controlled;
    }
}

[Serializable, NetSerializable]
public sealed class TargetingConsoleFireMessage : BoundUserInterfaceMessage
{
    public Vector2 Coordinates;

    public TargetingConsoleFireMessage(Vector2 coords)
    {
        Coordinates = coords;
    }
}

[Serializable, NetSerializable]
public sealed class TargetingConsoleGroupChangedMessage : BoundUserInterfaceMessage
{
    public string GroupName;

    public TargetingConsoleGroupChangedMessage(string name)
    {
        GroupName = name;
    }
}

[Serializable, NetSerializable]
public enum TargetingConsoleUiKey : byte
{
    Key,
}
