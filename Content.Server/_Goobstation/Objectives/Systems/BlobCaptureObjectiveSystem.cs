using Content.Server.Backmen.Blob.Components;
using Content.Server.Station.Systems;
using Content.Shared.Backmen.Blob.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server.Backmen.Objectives.Systems;

public sealed class BlobCaptureObjectiveSystem : EntitySystem
{
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly MetaDataSystem _metaDataSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobCaptureConditionComponent, ObjectiveGetProgressEvent>(OnBlobCaptureProgress);
        SubscribeLocalEvent<BlobCaptureConditionComponent, ObjectiveAfterAssignEvent>(OnBlobCaptureInfo);
        SubscribeLocalEvent<BlobCaptureConditionComponent, ObjectiveAssignedEvent>(OnBlobCaptureInfoAdd);
    }


    private void OnBlobCaptureInfoAdd(Entity<BlobCaptureConditionComponent> ent, ref ObjectiveAssignedEvent args)
    {
        if (args.Mind.OwnedEntity == null)
        {
            args.Cancelled = true;
            return;
        }
        if (!TryComp<BlobObserverComponent>(args.Mind.OwnedEntity, out var blobObserverComponent)
            || !HasComp<BlobCoreComponent>(blobObserverComponent.Core))
        {
            args.Cancelled = true;
            return;
        }

        var station = _stationSystem.GetOwningStation(blobObserverComponent.Core);
        if (station == null)
        {
            args.Cancelled = true;
            return;
        }

        ent.Comp.Target = CompOrNull<StationBlobConfigComponent>(station)?.StageTheEnd ?? StationBlobConfigComponent.DefaultStageEnd;
    }

    private void OnBlobCaptureInfo(EntityUid uid, BlobCaptureConditionComponent component, ref ObjectiveAfterAssignEvent args)
    {
        _metaDataSystem.SetEntityName(uid,Loc.GetString("objective-condition-blob-capture-title"));
        _metaDataSystem.SetEntityDescription(uid,Loc.GetString("objective-condition-blob-capture-description", ("count", component.Target)));
    }

    private void OnBlobCaptureProgress(EntityUid uid, BlobCaptureConditionComponent component, ref ObjectiveGetProgressEvent args)
    {
        if (!TryComp<BlobObserverComponent>(args.Mind.OwnedEntity, out var blobObserverComponent)
            || !TryComp<BlobCoreComponent>(blobObserverComponent.Core, out var blobCoreComponent))
        {
            args.Progress = 0;
            return;
        }

        var target = component.Target;
        args.Progress = 0;

        if (target != 0)
            args.Progress = MathF.Min((float) blobCoreComponent.BlobTiles.Count / target, 1f);
        else
            args.Progress = 1f;
    }
}
