using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Body.Events;
using Content.Shared.Body.Organ;
using Content.Server.DelayedDeath;
using Content.Server.Body.Components;
namespace Content.Server.Body.Systems;

public sealed class HeartSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeartComponent, OrganAddedToBodyEvent>(HandleAddition);
        SubscribeLocalEvent<HeartComponent, OrganRemovedFromBodyEvent>(HandleRemoval);
    }

    private void HandleRemoval(EntityUid uid, HeartComponent _, ref OrganRemovedFromBodyEvent args)
    {
        if (TerminatingOrDeleted(uid) || TerminatingOrDeleted(args.OldBody))
            return;

        // TODO: Add some form of very violent bleeding effect.
        EnsureComp<DelayedDeathComponent>(args.OldBody);
    }

    private void HandleAddition(EntityUid uid, HeartComponent _, ref OrganAddedToBodyEvent args)
    {
        if (TerminatingOrDeleted(uid) || TerminatingOrDeleted(args.Body))
            return;

        if (_bodySystem.TryGetBodyOrganComponents<BrainComponent>(args.Body, out var _))
            RemComp<DelayedDeathComponent>(args.Body);
    }
    // Shitmed-End
}