using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Arcadis.Computer;

/// <summary>
/// Main component for the ComputerDisk system
/// </summary>
[RegisterComponent, NetworkedComponent]
//[Access(typeof(ComputerDiskSystem))]
public sealed partial class ComputerDiskComponent : Component
{
    /// <summary>
    /// The prototype of the computer that will be used
    /// </summary>
    [DataField]
    public EntProtoId ProgramPrototype;
    public EntityUid? ProgramPrototypeEntity;

    [DataField]
    public bool PersistState;
}
