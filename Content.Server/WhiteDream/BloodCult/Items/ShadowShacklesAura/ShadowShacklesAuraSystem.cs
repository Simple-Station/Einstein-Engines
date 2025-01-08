using System.Linq;
using Content.Server.Chat.Systems;
using Content.Server.Cuffs;
using Content.Server.DoAfter;
using Content.Shared.Cuffs.Components;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Speech.Muting;
using Content.Shared.StatusEffect;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.WhiteDream.BloodCult.BloodCultist;
using Content.Shared.WhiteDream.BloodCult.Spells;
using Robust.Server.GameObjects;

namespace Content.Server.WhiteDream.BloodCult.Items.ShadowShacklesAura;

public sealed class ShadowShacklesAuraSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly CuffableSystem _cuffable = default!;
    [Dependency] private readonly DoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowShacklesAuraComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<ShadowShacklesAuraComponent, ShadowShacklesDoAfterEvent>(OnDoAfter);
    }

    private void OnMeleeHit(EntityUid uid, ShadowShacklesAuraComponent component, MeleeHitEvent args)
    {
        if (!args.HitEntities.Any())
            return;

        var target = args.HitEntities.First();
        if (uid == target || HasComp<BloodCultistComponent>(target) || !HasComp<CuffableComponent>(target))
            return;

        if (component.Speech != null)
            _chat.TrySendInGameICMessage(args.User, component.Speech, component.ChatType, false);

        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            args.User,
            component.Delay,
            new ShadowShacklesDoAfterEvent(),
            uid,
            target,
            uid)
        {
            DistanceThreshold = SharedInteractionSystem.InteractionRange,
            BreakOnDamage = true,
            BreakOnMove = true,
        };

        _doAfter.TryStartDoAfter(doAfterArgs, out _);
    }

    private void OnDoAfter(EntityUid uid, ShadowShacklesAuraComponent component, ShadowShacklesDoAfterEvent args)
    {
        if (!args.Target.HasValue)
            return;

        var shackles = Spawn(component.ShacklesProto, _transform.GetMapCoordinates(args.User));
        if (!_cuffable.TryAddNewCuffs(args.Target.Value, args.User, shackles))
        {
            QueueDel(shackles);
            return;
        }

        _statusEffects.TryAddStatusEffect<MutedComponent>(args.Target.Value, "Muted", component.MuteDuration, true);
        QueueDel(uid);
    }
}
