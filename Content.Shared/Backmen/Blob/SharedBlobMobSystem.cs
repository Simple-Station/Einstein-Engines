using System.Numerics;
using Content.Shared.Backmen.Blob.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared.Backmen.Blob;

public abstract class SharedBlobMobSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    private EntityQuery<BlobTileComponent> _tileQuery;
    private EntityQuery<BlobMobComponent> _mobQuery;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobMobComponent, AttackAttemptEvent>(OnBlobAttackAttempt);
        SubscribeNetworkEvent<BlobMobGetPulseEvent>(OnPulse);
        _tileQuery = GetEntityQuery<BlobTileComponent>();
        _mobQuery = GetEntityQuery<BlobMobComponent>();
    }


    [ValidatePrototypeId<EntityPrototype>]
    private const string HealEffect = "EffectHearts";

    private readonly SoundSpecifier _healAudio = new SoundPathSpecifier("/Audio/Backmen/Ambience/blob_heal.ogg");

    private void OnPulse(BlobMobGetPulseEvent ev)
    {
        if(!TryGetEntity(ev.BlobEntity, out var blobEntity))
            return;

        _audioSystem.PlayPredicted(_healAudio, blobEntity.Value, null, AudioParams.Default.WithMaxDistance(4).WithVolume(0.5f));
        SpawnAttachedTo(HealEffect, new EntityCoordinates(blobEntity.Value, Vector2.Zero));
    }

    private void OnBlobAttackAttempt(EntityUid uid, BlobMobComponent component, AttackAttemptEvent args)
    {
        if (args.Cancelled || !_tileQuery.HasComp(args.Target) && !_mobQuery.HasComp(args.Target))
            return;

        _popupSystem.PopupCursor(Loc.GetString("blob-mob-attack-blob"), PopupType.Large);
        args.Cancel();
    }
}
