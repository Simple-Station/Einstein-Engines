using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.BUIStates;

[Serializable, NetSerializable]
public enum ShuttleConsoleAccesState
{
    // Always logged in on NotDynamic , since we dont have dynamic acces reader
    NotDynamic,
    NoAcces,
    PilotAcces,
    CaptainAcces,
};

[Serializable, NetSerializable]
public sealed class SwitchedToCrewHudMessage(bool visible) : BoundUserInterfaceMessage
{
    public bool Visible = visible;
}

[Serializable, NetSerializable]
public sealed class TryMakeEmployeeMessage(string option) : BoundUserInterfaceMessage
{
    public string chosenOption = option;
}

[Serializable, NetSerializable]
public sealed class CrewInterfaceState
{
    public bool hasId;
    public string IdName;
    public HashSet<string>? IdCodes;
    public HashSet<string>? Pressed;


    public CrewInterfaceState(string name, HashSet<string>? codes)
    {
        IdName = name;
        IdCodes = codes;
    }
}
