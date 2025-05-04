using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Arcadis.Computer;

/// <summary>
/// Component responsible for handling DiskBurner behaviour
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DiskBurnerComponent : Component {

    [DataField]
    public string DiskSlot = "diskSlot";

    [DataField]
    public string BoardSlot = "boardSlot";

    [DataField]
    public string VerbName = "Burn Disk";
}
