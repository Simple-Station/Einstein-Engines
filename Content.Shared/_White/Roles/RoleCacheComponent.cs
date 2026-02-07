using Content.Shared.Roles;
using Robust.Shared.Prototypes;


namespace Content.Shared._White.Roles;

[RegisterComponent]
public sealed partial class RoleCacheComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), Access(typeof(RolesCacheSystem))]
    public int AntagWeight;

    [ViewVariables(VVAccess.ReadOnly)]
    public bool IsAntag => AntagWeight > 0;

    [ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<AntagPrototype>? LastAntagPrototype { get; [Access(typeof(RolesCacheSystem))] set; }

    [ViewVariables(VVAccess.ReadOnly)]
    public ProtoId<JobPrototype>? LastJobPrototype { get; [Access(typeof(RolesCacheSystem))] set; }
}
