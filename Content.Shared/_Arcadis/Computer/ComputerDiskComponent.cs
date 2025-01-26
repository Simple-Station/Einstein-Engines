using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Arcadis.Computer;

/// <summary>
/// Main component for the ComputerDisk system
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
//[Access(typeof(ComputerDiskSystem))]
public sealed partial class ComputerDiskComponent : Component
{
    /// <summary>
    /// The prototype of the computer that will be used
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId ProgramPrototype;

    [AutoNetworkedField]

    public EntityUid? ProgramPrototypeEntity;

    [DataField, AutoNetworkedField]
    public bool PersistState;
}
