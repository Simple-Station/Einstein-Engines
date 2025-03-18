using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Roles.Jobs;

/// <summary>
///     Added to mind role entities to mark them as a job role entity.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class JobRoleComponent : BaseMindRoleComponent
{
    [DataField(required: true), AutoNetworkedField]
    public ProtoId<JobPrototype>? Prototype;
}
