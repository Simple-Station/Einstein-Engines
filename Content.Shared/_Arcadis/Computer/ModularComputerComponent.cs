using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Arcadis.Computer;

/// <summary>
/// Main component for the ComputerDisk system
/// </summary>
[RegisterComponent, NetworkedComponent]
//[Access(typeof(ComputerDiskSystem))]
public sealed partial class ModularComputerComponent : Component
{
    [DataField]
    public string DiskSlot = "modularComputerDiskSlot";

    [DataField]
    public SoundSpecifier? DiskInsertSound = new SoundPathSpecifier("/Audio/_Arcadis/computer_startup.ogg");

    [DataField]

    public bool RequiresPower = true;
}
