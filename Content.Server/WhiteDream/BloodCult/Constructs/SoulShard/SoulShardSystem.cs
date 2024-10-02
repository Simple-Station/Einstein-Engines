using Content.Server.Bible.Components;
using Content.Server.Popups;
using Content.Server.Roles;
using Content.Shared.Interaction;
using Content.Shared.Mind.Components;
using Content.Shared.Roles;
using Content.Shared.WhiteDream.BloodCult;
using Robust.Server.Audio;
using Robust.Server.GameObjects;

namespace Content.Server.WhiteDream.BloodCult.Constructs.SoulShard;

public sealed class SoulShardSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedPointLightSystem _lightSystem = default!;
    [Dependency] private readonly SharedRoleSystem _roleSystem = default!;
    [Dependency] private readonly PopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SoulShardComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<SoulShardComponent, MindAddedMessage>(OnShardMindAdded);
        SubscribeLocalEvent<SoulShardComponent, MindRemovedMessage>(OnShardMindRemoved);
    }

    private void OnInteractUsing(Entity<SoulShardComponent> shard, ref InteractUsingEvent args)
    {
        if (shard.Comp.IsBlessed || !TryComp(args.Used, out BibleComponent? bible))
            return;

        _popup.PopupEntity(Loc.GetString("bible-sizzle"), args.User, args.User);
        _audio.PlayPvs(bible.HealSoundPath, args.User);
        _appearanceSystem.SetData(shard, SoulShardVisualState.Blessed, true);
        shard.Comp.IsBlessed = true;
    }

    private void OnShardMindAdded(Entity<SoulShardComponent> shard, ref MindAddedMessage args)
    {
        if (!TryComp<MindContainerComponent>(shard, out var mindContainer) || !mindContainer.HasMind)
            return;

        _roleSystem.MindTryRemoveRole<TraitorRoleComponent>(mindContainer.Mind.Value);
        _appearanceSystem.SetData(shard, SoulShardVisualState.HasMind, true);
        _lightSystem.SetEnabled(shard, true);
        _lightSystem.SetColor(shard, shard.Comp.IsBlessed ? shard.Comp.BlessedLightColor : shard.Comp.LightColor);
    }

    private void OnShardMindRemoved(Entity<SoulShardComponent> shard, ref MindRemovedMessage args)
    {
        _appearanceSystem.SetData(shard, SoulShardVisualState.HasMind, false);
        _lightSystem.SetEnabled(shard, false);
    }
}
