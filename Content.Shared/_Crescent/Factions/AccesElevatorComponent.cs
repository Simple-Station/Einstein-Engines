using Content.Shared.Access;
using Robust.Shared.Prototypes;

namespace Content.Shared.Roles;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class AccesElevatorComponent : Component
{
    [DataField("acces"), ViewVariables]
    public List<ProtoId<AccessLevelPrototype>> giveAcces = default!;
}
