using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using System.Numerics;

namespace Content.Shared._Orehum.ME4TA.Vergil;

public sealed class VergilAbilitiesSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VergilAbilitiesComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<VergilAbilitiesComponent, VergilJudgementCutEvent>(OnJudgementCut);
        SubscribeLocalEvent<VergilAbilitiesComponent, VergilDashEvent>(OnVergilDash);
    }

    private void OnMapInit(EntityUid uid, VergilAbilitiesComponent component, MapInitEvent args)
    {
        _actions.AddAction(uid, ref component.DashActionEntity, "ActionVergilDash");
        _actions.AddAction(uid, ref component.JCEntity, "ActionVergilJudgementCut");
    }

    private bool IsHoldingYamato(EntityUid uid)
    {
        foreach (var hand in _hands.EnumerateHands(uid))
        {
            if (hand.HeldEntity is not { Valid: true } held)
                continue;

            if (_tag.HasTag(held, "Yamato"))
                return true;
        }
        return false;
    }

    private void OnJudgementCut(EntityUid uid, VergilAbilitiesComponent component, VergilJudgementCutEvent args)
    {
        if (args.Handled) return;

        if (!IsHoldingYamato(uid))
        {
            _popup.PopupEntity("Тебе нужно Ямато, чтобы сделать это!", uid, uid);
            return;
        }

        if (!_interaction.InRangeUnobstructed(uid, args.Target, range: 9f))
            return;

        var damage = new DamageSpecifier();
        damage.DamageDict.Add("Slash", 13);

        var mapPos = args.Target.ToMap(EntityManager, _transform);
        var targets = _lookup.GetEntitiesInRange(mapPos, 2.5f);

        var hitList = new HashSet<EntityUid>();

        foreach (var target in targets)
        {
            if (target == uid || !HasComp<DamageableComponent>(target))
                continue;

            var actualTarget = target;
            if (hitList.Contains(actualTarget))
                continue;

            var changed = _damageable.TryChangeDamage(actualTarget, damage, ignoreResistances: true);

            if (changed != null)
            {
                hitList.Add(actualTarget);
                _popup.PopupEntity($"-13 HP", actualTarget, PopupType.SmallCaution);
            }
        }

        _audio.PlayPvs("/Audio/Weapons/plasma_cutter.ogg", args.Target);
        _entManager.SpawnEntity("ActionVergilJudgementCutEffect", args.Target);

        _stamina.TakeStaminaDamage(uid, 35f);
        args.Handled = true;
    }

    private void OnVergilDash(EntityUid uid, VergilAbilitiesComponent component, VergilDashEvent args)
    {
        if (args.Handled) return;

        if (!IsHoldingYamato(uid))
        {
            _popup.PopupEntity("Тебе нужно Ямато, чтобы сделать это!", uid, uid);
            return;
        }

        if (!_interaction.InRangeUnobstructed(uid, args.Target, range: 12f))
        {
            return;
        }

        _stamina.TakeStaminaDamage(uid, 25f);

        _audio.PlayPvs("/Audio/Magic/blink.ogg", uid);
        _transform.SetCoordinates(uid, args.Target);

        args.Handled = true;
    }
}
