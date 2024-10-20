using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.WhiteDream.BloodCult.Constructs.SoulShard;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Mind.Components;
using Content.Shared.RadialSelector;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Utility;

namespace Content.Server.WhiteDream.BloodCult.Constructs.Shell;

public sealed class ConstructShellSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _slots = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ConstructShellComponent, GetVerbsEvent<ExamineVerb>>(OnGetVerbs);
        SubscribeLocalEvent<ConstructShellComponent, ComponentInit>(OnShellInit);
        SubscribeLocalEvent<ConstructShellComponent, ContainerIsInsertingAttemptEvent>(OnInteractUsing);
        SubscribeLocalEvent<ConstructShellComponent, RadialSelectorSelectedMessage>(OnConstructSelected);
        SubscribeLocalEvent<ConstructShellComponent, ComponentRemove>(OnShellRemove);
    }

    private void OnGetVerbs(Entity<ConstructShellComponent> shell, ref GetVerbsEvent<ExamineVerb> args)
    {
        var shellUid = shell.Owner;
        if (_slots.GetItemOrNull(shell, shell.Comp.ShardSlotId) is not { } shard || args.User != shard ||
            !TryComp(shard, out SoulShardComponent? soulShard) || _ui.IsUiOpen(shellUid, RadialSelectorUiKey.Key))
            return;

        args.Verbs.Add(new ExamineVerb
        {
            DoContactInteraction = true,
            Text = Loc.GetString("soul-shard-selector-form"),
            Icon = new SpriteSpecifier.Texture(
                new ResPath("/Textures/WhiteDream/BloodCult/Entities/Items/construct_shell.rsi")),
            Act = () =>
            {
                _ui.SetUiState(shellUid,
                    RadialSelectorUiKey.Key,
                    new RadialSelectorState(soulShard.IsBlessed ? soulShard.PurifiedConstructs : soulShard.Constructs,
                        true));
                _ui.TryToggleUi(shellUid, RadialSelectorUiKey.Key, shard);
            }
        });
    }

    private void OnShellInit(Entity<ConstructShellComponent> shell, ref ComponentInit args)
    {
        _slots.AddItemSlot(shell, shell.Comp.ShardSlotId, shell.Comp.ShardSlot);
    }

    private void OnInteractUsing(Entity<ConstructShellComponent> shell, ref ContainerIsInsertingAttemptEvent args)
    {
        var shellUid = shell.Owner;
        if (!TryComp(args.EntityUid, out SoulShardComponent? soulShard) ||
            _ui.IsUiOpen(shellUid, RadialSelectorUiKey.Key))
            return;

        if (!TryComp<MindContainerComponent>(args.EntityUid, out var mindContainer) || !mindContainer.HasMind )
        {
            _popup.PopupEntity(Loc.GetString("soul-shard-try-insert-no-soul"), shell);
            args.Cancel();
            return;
        }

        _slots.SetLock(shell, shell.Comp.ShardSlotId, true);
        _ui.SetUiState(shellUid,
            RadialSelectorUiKey.Key,
            new RadialSelectorState(soulShard.IsBlessed ? soulShard.PurifiedConstructs : soulShard.Constructs, true));

        _ui.TryToggleUi(shellUid, RadialSelectorUiKey.Key, args.EntityUid);
    }

    private void OnConstructSelected(Entity<ConstructShellComponent> ent, ref RadialSelectorSelectedMessage args)
    {
        if (!_mind.TryGetMind(args.Actor, out var mindId, out _))
            return;

        var construct = Spawn(args.SelectedItem, _transform.GetMapCoordinates(ent));
        _mind.TransferTo(mindId, construct);
        Del(ent);
    }

    private void OnShellRemove(Entity<ConstructShellComponent> shell, ref ComponentRemove args)
    {
        _slots.RemoveItemSlot(shell, shell.Comp.ShardSlot);
    }
}
