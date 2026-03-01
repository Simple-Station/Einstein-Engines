using Robust.Shared.Serialization;
using Content.Shared.Actions;

namespace Content.Shared.Silicons.StationAi;

[Serializable, NetSerializable]
public enum StationAiInfoUiKey : byte
{
    Key
}

public sealed partial class StationAiInfoActionEvent : InstantActionEvent
{
}


[Serializable, NetSerializable]
public sealed class RoboticsControlOpenUiMessage : BoundUserInterfaceMessage
{
}
