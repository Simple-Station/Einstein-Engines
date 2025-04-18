using System.Numerics;
using Content.Server.Actions;
using Content.Server.Camera;
using Content.Server.Stunnable;
using Content.Shared._EE.Shadowling;
using Content.Shared.Camera;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Content.Shared.Silicon.Components;
using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Tag;
using Robust.Shared.Random;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles the Sonic Screech ability logic.
/// Sonic Screech "confuses" and "deafens" (flash effect + tinnitus sound) nearby people, damages windows, and stuns silicons/borgs. All in one pack!
/// </summary>
public sealed class ShadowlingSonicScreechSystem : EntitySystem
{
    /// <inheritdoc/>
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly CameraRecoilSystem _cameraRecoil = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ShadowlingSonicScreechComponent, SonicScreechEvent>(OnSonicScreech);
    }

    private void OnSonicScreech(EntityUid uid, ShadowlingSonicScreechComponent component, SonicScreechEvent args)
    {
        foreach (var entity in _lookup.GetEntitiesInRange(uid, component.Range))
        {
            if (HasComp<ThrallComponent>(entity) || HasComp<ShadowlingComponent>(entity))
                continue;

            TryStunSilicons(entity, component);
            TryDamageWindows(entity, component);
            TryConfusePlayer(entity, uid, component);
        }

        _actions.StartUseDelay(args.Action);
    }

    private void TryStunSilicons(EntityUid target, ShadowlingSonicScreechComponent component)
    {
        if (!HasComp<SiliconComponent>(target) || !HasComp<BorgChassisComponent>(target))
            return;

        _stun.TryParalyze(target, component.SiliconStunTime, false);
    }

    private void TryDamageWindows(EntityUid target, ShadowlingSonicScreechComponent component)
    {
        if (_tag.HasTag(target, component.WindowTag))
        {
            if (!TryComp<DamageableComponent>(target, out var damageableComponent))
                return;

            _damageable.TryChangeDamage(
                target,
                component.WindowDamage,
                true,
                damageable: damageableComponent
            );
        }
    }

    private void TryConfusePlayer(EntityUid target, EntityUid user, ShadowlingSonicScreechComponent component)
    {
        if (!HasComp<HumanoidAppearanceComponent>(target))
            return;

        if (!TryComp<CameraRecoilComponent>(target, out var recoil))
            return;

        // I'm not gonna implement deafening for now (at least until ear component gets its deafen logic by someone),
        // so it will just play a tinnitus sound and spawn a short flash effect on players.
        var kick = new Vector2(_random.NextFloat(), _random.NextFloat()) * component.ScreechKick;
        _cameraRecoil.KickCamera(target, kick, recoil);

        EntityManager.SpawnAtPosition(component.ProtoFlash, Transform(target).Coordinates); // todo: change this to custom flash
    }
}
