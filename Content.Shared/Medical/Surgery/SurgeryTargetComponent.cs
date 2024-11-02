using Robust.Shared.GameStates;

namespace Content.Shared.Medical.Surgery;

[RegisterComponent, NetworkedComponent]
public sealed partial class SurgeryTargetComponent : Component
{
    [DataField]
    public bool CanOperate = true;
}
