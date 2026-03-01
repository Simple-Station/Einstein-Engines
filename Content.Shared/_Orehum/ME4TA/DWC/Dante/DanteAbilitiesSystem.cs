using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Eye;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Random;
using System.Linq;
using System.Numerics;

namespace Content.Shared._Orehum.ME4TA.Dante;

public sealed class DanteAbilitiesSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DanteAbilitiesComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<DanteAbilitiesComponent, DanteRainStormEvent>(OnRainStorm);
        SubscribeLocalEvent<DanteAbilitiesComponent, DanteStingerEvent>(OnStinger);
    }

    private void OnMapInit(EntityUid uid, DanteAbilitiesComponent component, MapInitEvent args)
    {
        _actions.AddAction(uid, ref component.RainStormActionEntity, "ActionDanteRainStorm");
        _actions.AddAction(uid, ref component.StingerActionEntity, "ActionDanteStinger");
    }

    private void OnRainStorm(EntityUid uid, DanteAbilitiesComponent component, DanteRainStormEvent args)
    {
        if (args.Handled) return;

        if (!IsHoldingGuns(uid))
        {
            _popup.PopupEntity("Где мои пистолеты?!", uid, uid);
            return;
        }

        _audio.PlayPvs("/Audio/Weapons/Guns/Gunshots/pistol.ogg", uid);

        var xform = Transform(uid);
        var playerPos = _transform.GetMapCoordinates(uid).Position;
        var outerRadius = 4.5f;
        var innerRadius = 1.0f;
        var entities = _lookup.GetEntitiesInRange(_transform.GetMapCoordinates(uid), outerRadius);

        DamageSpecifier damage = new();
        damage.DamageDict.Add("Piercing", 15);

        foreach (var target in entities)
        {
            if (target == uid) continue;
            if (!HasComp<DamageableComponent>(target)) continue;

            var targetPos = _transform.GetMapCoordinates(target).Position;
            var distance = (targetPos - playerPos).Length();

            if (distance < innerRadius)
                continue;

            _damageable.TryChangeDamage(target, damage, ignoreResistances: false);
            _entManager.SpawnEntity("ActionDanteRainEffect", Transform(target).Coordinates);
        }

        args.Handled = true;
    }

    private bool IsHoldingGuns(EntityUid uid)
    {
        int guns = 0;
        foreach (var hand in _hands.EnumerateHands(uid))
        {
            if (hand.HeldEntity is { Valid: true } held && _tag.HasTag(held, "EbonyIvory"))
                guns++;
        }
        return guns >= 2;
    }

    private void OnStinger(EntityUid uid, DanteAbilitiesComponent component, DanteStingerEvent args)
    {
        if (args.Handled) return;

        if (!_hands.TryGetActiveHand(uid, out var hand) || hand.HeldEntity == null || !_tag.HasTag(hand.HeldEntity.Value, "Rebellion"))
        {
            _popup.PopupEntity("Мне нужен Мятежник!", uid, uid);
            return;
        }

        var xform = Transform(uid);
        var originMap = _transform.GetMapCoordinates(uid);
        var origin = originMap.Position;
        var targetMap = _transform.ToMapCoordinates(args.Target);
        var targetPos = targetMap.Position;

        if (targetMap.MapId != originMap.MapId)
            return;

        var direction = targetPos - origin;
        var length = direction.Length();
        var maxDist = 7f;
        if (length > maxDist)
        {
            direction = direction.Normalized() * maxDist;
            targetPos = origin + direction;
        }
        else if (length < 0.1f) return;

        var ray = new CollisionRay(origin, direction.Normalized(), (int)(CollisionGroup.Impassable | CollisionGroup.MobLayer));
        var rayCastResults = _physics.IntersectRay(originMap.MapId, ray, maxDist, uid, false).ToList();

        EntityUid? hitEntity = null;
        float hitDistance = maxDist;

        foreach (var result in rayCastResults)
        {
            if (result.HitEntity == uid) continue;
            hitEntity = result.HitEntity;
            hitDistance = result.Distance;
            break;
        }

        var newPos = origin + direction.Normalized() * Math.Max(0, hitDistance - 0.5f);
        _transform.SetMapCoordinates(uid, new MapCoordinates(newPos, originMap.MapId));

        if (hitEntity != null)
        {
            if (HasComp<DamageableComponent>(hitEntity.Value))
            {
                var damage = new DamageSpecifier();
                damage.DamageDict.Add("Slash", 17);
                damage.DamageDict.Add("Piercing", 5);
                _damageable.TryChangeDamage(hitEntity.Value, damage, ignoreResistances: false);
                _audio.PlayPvs("/Audio/Weapons/star_hit.ogg", hitEntity.Value);
                _popup.PopupEntity("Stinger!", hitEntity.Value, PopupType.Medium);
            }
            else
            {
                _stamina.TakeStaminaDamage(uid, 150f);
                _popup.PopupEntity("Ты врезался в стену!", uid, PopupType.LargeCaution);
                _audio.PlayPvs("/Audio/Effects/thudswoosh.ogg", uid);
            }
        }

        args.Handled = true;
    }

}
