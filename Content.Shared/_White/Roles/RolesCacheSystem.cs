using Content.Shared.Mind;
using Content.Shared.Roles;


namespace Content.Shared._White.Roles;


public sealed class RolesCacheSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<MindComponent, RoleAddingEvent>(OnRoleAdded);
        SubscribeLocalEvent<MindComponent, RoleRemovingEvent>(OnRoleRemoved);
    }

    private void OnRoleRemoved(EntityUid uid, MindComponent mind, RoleRemovingEvent args)
    {
        if (!TryComp<RoleCacheComponent>(mind.CurrentEntity, out var component))
            return;

        if (args.RoleComponent.Antag)
            component.AntagWeight -= 1;
    }

    private void OnRoleAdded(EntityUid uid, MindComponent component, RoleAddingEvent args)
    {
        if (component.CurrentEntity is null)
            return;

        var cacheComp = EnsureComp<RoleCacheComponent>(component.CurrentEntity.Value);

        if (args.RoleComponent.Antag)
            cacheComp.AntagWeight += 1;

        if (args.RoleComponent.JobPrototype != null)
            cacheComp.LastJobPrototype = args.RoleComponent.JobPrototype;
        if (args.RoleComponent.AntagPrototype != null)
            cacheComp.LastAntagPrototype = args.RoleComponent.AntagPrototype;
    }
}
