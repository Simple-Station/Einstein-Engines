using Content.Shared.Mind;
using Content.Shared.Roles;


namespace Content.Shared._White.Roles;


public sealed record RoleRemovingEvent(EntityUid MindId, MindComponent MindComponent, EntityUid RoleUid, MindRoleComponent RoleComponent) : RoleEvent(MindId, MindComponent, false);
