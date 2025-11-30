using Content.Shared.Shuttles.Components;
using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.BUIStates;

[Serializable, NetSerializable]
public sealed class IFFConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    public IFFFlags AllowedFlags;
    public IFFFlags Flags;
    public float HeatCapacity;
    public float CurrentHeat;

    // The color adding
    public Color Color;
    public bool AllowColorChange;
}

[Serializable, NetSerializable]
public enum IFFConsoleUiKey : byte
{
    Key,
}
