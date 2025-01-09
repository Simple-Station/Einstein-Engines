using Content.Shared.Backmen.Blob.Components;
using Content.Shared.Verbs;

namespace Content.Shared.Backmen.Blob;

public abstract class SharedBlobTileSystem : EntitySystem
{
    protected EntityQuery<BlobObserverComponent> ObserverQuery;
    protected EntityQuery<BlobCoreComponent> CoreQuery;
    protected EntityQuery<TransformComponent> TransformQuery;
    protected EntityQuery<BlobTileComponent> TileQuery;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobUpgradeableTileComponent, GetVerbsEvent<AlternativeVerb>>(AddUpgradeVerb);

        ObserverQuery = GetEntityQuery<BlobObserverComponent>();
        CoreQuery = GetEntityQuery<BlobCoreComponent>();
        TransformQuery = GetEntityQuery<TransformComponent>();
        TileQuery = GetEntityQuery<BlobTileComponent>();
    }

    protected abstract void TryUpgrade(Entity<BlobTileComponent, BlobUpgradeableTileComponent> target, Entity<BlobCoreComponent> core, EntityUid observer);

    private void AddUpgradeVerb(EntityUid uid, BlobUpgradeableTileComponent comp, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!ObserverQuery.TryGetComponent(args.User, out var ghostBlobComponent) ||
            !TileQuery.TryGetComponent(uid, out var component) ||
            TransformQuery.TryGetComponent(uid, out var transformComponent) && !transformComponent.Anchored ||
            ghostBlobComponent.Core == null ||
            component.Core == null ||
            !CoreQuery.HasComponent(ghostBlobComponent.Core.Value))
            return;

        var verbName = Loc.GetString(comp.Locale);

        AlternativeVerb verb = new()
        {
            Act = () => TryUpgrade((uid, component, comp), ghostBlobComponent.Core.Value, args.User),
            Text = verbName,
        };
        args.Verbs.Add(verb);
    }
}
