using Content.Shared.Actions;
using Content.Shared.EntityTable;
using Content.Shared.EntityTable.EntitySelectors;
using Content.Shared.Examine;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Content.Server.Storage.Components;
using Content.Shared.Storage.EntitySystems;
using Content.Shared.Storage.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Content.Shared._Orehum.Summonable;

namespace Content.Server._Orehum.Summonable;

public sealed class SharedContainerSummonableSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedEntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly EntityTableSystem _entityTable = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ContainerSummonActionEvent>(OnContainerSummonAction);
    }


    private void OnContainerSummonAction(ContainerSummonActionEvent args)
    {
        if (!TryComp<EntityStorageComponent>(args.Target, out var storage) ||
            _entityStorage.IsOpen(args.Target, storage))
            return;


        var spawns = _entityTable.GetSpawns(args.Table);
        foreach (var spawn in spawns)
        {
            var ent = Spawn(spawn, _transform.GetMapCoordinates(args.Target));
            _entityStorage.Insert(ent, args.Target, storage);
        }

        _popup.PopupEntity(Loc.GetString("container-summonable-summon-popup", ("target", args.Target)), args.Target, args.Performer);
        _audio.PlayEntity(args.SummonSound, args.Target, args.Performer);

        args.Handled = true;
    }
}
