using Content.Shared.Examine;
using Content.Shared.Weapons.Ranged.Systems;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Ranged.Components;
using Robust.Shared.Containers;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Random;
using Robust.Shared.Network;
using Content.Shared.Throwing;
using Content.Shared.Audio;
using Robust.Shared.Utility;
using Content.Shared.Weapons.Reflect;
using Content.Shared.Weapons.Melee.Events;
using System.Numerics;

namespace Content.Shared._Arcadis.Limbus;

public sealed class GunSwordSystem : EntitySystem
{
    /* TODO: You have a lot of work to do here.
        - Make the empty gun verb work
        - Make having shells increase power based on the highest non-blank shell
        - Add some extra visuals, maybe?
        - Send demo to Cross
    */
    [Dependency] protected readonly SharedContainerSystem _container = default!;
    [Dependency] protected readonly ThrowingSystem _throw = default!;
    [Dependency] protected readonly IRobustRandom _random = default!;
    [Dependency] private readonly INetManager _netMan = default!;
    [Dependency] protected readonly SharedGunSystem _gun = default!;
    [Dependency] protected readonly SharedAudioSystem _audio = default!;
    [Dependency] protected readonly SharedTransformSystem _transform = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<GunSwordComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeLocalEvent<GunSwordComponent, MeleeHitEvent>(OnMeleeHit);
        base.Initialize();
    }

    public void OnMeleeHit(EntityUid uid, GunSwordComponent component, MeleeHitEvent args)
    {
        if (!TryComp<BallisticAmmoProviderComponent>(uid, out var ballistic))
            return;

        int shellsUsed = 0;
        float knockbackForce = 0f;

        while (shellsUsed < component.ShellsPerHit && GetShell(ballistic) != null)
        {
            var shellEntity = GetShell(ballistic);
            if (shellEntity == null)
                break;

            if (!TryComp<CartridgeAmmoComponent>(shellEntity, out var ammo) || ammo.Spent)
                continue;

            if (!TryComp<GunSwordAmmoComponent>(shellEntity, out var shellComp))
                continue;

            args.BonusDamage += shellComp.DamageAmplifier * args.BaseDamage;
            knockbackForce += shellComp.KnockbackForce;

            _gun.SetCartridgeSpent(shellEntity.Value, ammo, true);

            shellsUsed++;
        }

        if (knockbackForce > 0f)
        {
            foreach (var hit in args.HitEntities)
            {
                var attackerPos = _transform.GetWorldPosition(uid);
                var targetPos = _transform.GetWorldPosition(hit);
                var delta = targetPos - attackerPos;
                var normalizedDelta = Vector2.Normalize(delta);
                var flingVector = normalizedDelta * knockbackForce;
                _throw.TryThrow(hit, flingVector, 25f);
            }
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var query = EntityQueryEnumerator<GunSwordComponent, BallisticAmmoProviderComponent>();

        while (query.MoveNext(out var uid, out var comp, out var ballistic))
        {
            var reflectChance = 0f;

            foreach (var shellEntity in ballistic.Entities)
            {
                if (!TryComp<GunSwordAmmoComponent>(shellEntity, out var shellComp))
                    continue;

                if (!TryComp<CartridgeAmmoComponent>(shellEntity, out var ammo) || ammo.Spent)
                    continue;

                reflectChance += shellComp.ReflectChanceIncrease;
            }

            if (!TryComp<ReflectComponent>(uid, out var reflect))
                continue;

            reflect.ReflectProb = reflectChance;
        }

    }
    private void OnGetVerbs(EntityUid uid, GunSwordComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        if (!TryComp<BallisticAmmoProviderComponent>(uid, out var ballisticComponent))
            return;

        args.Verbs.Add(new()
        {
            Text = Loc.GetString("Cycle"),
            Disabled = GetShell(ballisticComponent, false) == null,
            Act = () => EmptyChamber(uid, ballisticComponent, component, args.User),
        });
    }

    private EntityUid? GetShell(BallisticAmmoProviderComponent component, bool? checkSpent = true)
    {
        for (var i = 0; i < component.Entities.Count; i++)
        {
            if (TryComp<CartridgeAmmoComponent>(component.Entities[i], out var ammo) && (!ammo.Spent || checkSpent == false))
            {
                return component.Entities[i];
            }
        }

        return null;
    }
    public void EmptyChamber(EntityUid entity, BallisticAmmoProviderComponent ballisticAmmo, GunSwordComponent gunSword, EntityUid user)
    {
        var anyEmpty = false;

        foreach (var shell in ballisticAmmo.Entities.ShallowClone())
        {
            _container.Remove(shell, ballisticAmmo.Container);

            if (_netMan.IsServer)
                _gun.EjectCartridge(shell);

            ballisticAmmo.Entities.Remove(shell);

            anyEmpty = true;
        }

        if (anyEmpty)
        {
            _audio.PlayPredicted(gunSword.SoundEject, entity, user);
            _gun.UpdateAmmoCount(entity, prediction: false);
            Dirty(entity, ballisticAmmo);
        }
    }
}
