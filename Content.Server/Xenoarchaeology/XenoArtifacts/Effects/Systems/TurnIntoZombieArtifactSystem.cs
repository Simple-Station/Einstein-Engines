using Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;
using Content.Server.Xenoarchaeology.XenoArtifacts.Events;
using Content.Server.Zombies;
using Content.Shared.Mind.Components;
using Robust.Shared.Player;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Systems;

public sealed class TurnIntoZombieArtifactSystem : EntitySystem
{
    [Dependency] private readonly ZombieSystem _zombie = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<TurnIntoZombieArtifactComponent, ArtifactActivatedEvent>(OnActivate);
    }

    private void OnActivate(EntityUid uid, TurnIntoZombieArtifactComponent comp, ArtifactActivatedEvent args)
    {
        if (args.Activator is null)
            return;

        if (!HasComp<MindContainerComponent>(args.Activator) || !TryComp<ActorComponent>(args.Activator, out var target))
            return;

        _zombie.ZombifyEntity((EntityUid) args.Activator);
    }
}
