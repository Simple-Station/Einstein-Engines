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

namespace Content.Shared._Arcadis.Limbus;

public abstract class GunSwordSystem : EntitySystem
{
    /* TODO: You have a lot of work to do here.
        - Make the empty gun verb work
        - Make having shells increase power based on the highest non-blank shell
        - Add some extra visuals, maybe?
        - Send demo to Cross
    */

    [Dependency] protected readonly ThrowingSystem _throw = default!;
    [Dependency] protected readonly IRobustRandom _random = default!;
    [Dependency] protected readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly INetManager _netMan = default!;
    [Dependency] protected readonly SharedGunSystem _gun = default!;
    [Dependency] protected readonly SharedAudioSystem _audio = default!;
    [Dependency] protected readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<GunSwordComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
    }

    private void OnGetVerbs(EntityUid uid, GunSwordComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands == null)
            return;

        if (!TryComp<BallisticAmmoProviderComponent>(uid, out var ballisticComponent))
            return;

        args.Verbs.Add(new AlternativeVerb()
        {
            Text = Loc.GetString("gun-revolver-empty"),
            Disabled = GetShell(ballisticComponent) == null,
            Act = () => EmptyChamber(uid, ballisticComponent, component, args.User),
            Priority = 1
        });
    }

    private EntityUid? GetShell(BallisticAmmoProviderComponent component)
    {
        for (var i = 0; i < component.Entities.Count; i++)
        {
            if (TryComp<CartridgeAmmoComponent>(component.Entities[i], out var ammo) && !ammo.Spent)
            {
                return component.Entities[i];
            }
        }

        return null;
    }

    public void EmptyChamber(EntityUid entity, BallisticAmmoProviderComponent ballisticAmmo, GunSwordComponent gunSword, EntityUid user)
    {
        var mapCoordinates = _transform.GetMapCoordinates(entity);
        var anyEmpty = false;

        for (var i = 0; i < ballisticAmmo.Capacity; i++)
        {
            var shell = ballisticAmmo.Entities[i];

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

    protected void EjectCartridge(
        EntityUid entity,
        Angle? angle = null,
        bool playSound = true,
        GunComponent? gunComp = null)
    {
        var throwingForce = 0.01f;
        var throwingSpeed = 5f;
        var ejectAngleOffset = 3.7f;
        if (gunComp is not null)
        {
            throwingForce = gunComp.EjectionForce;
            throwingSpeed = gunComp.EjectionSpeed;
            ejectAngleOffset = gunComp.EjectAngleOffset;
        }

        // TODO: Sound limit version.
        var offsetPos = _random.NextVector2(0.4f); // im too lazy to unhardcode this.
        var xform = Transform(entity);

        var coordinates = xform.Coordinates;
        coordinates = coordinates.Offset(offsetPos);

        _transform.SetLocalRotation(entity, _random.NextAngle(), xform);
        _transform.SetCoordinates(entity, xform, coordinates);
        if (angle is null)
            angle = _random.NextAngle();

        Angle ejectAngle = angle.Value;
        ejectAngle += ejectAngleOffset; // 212 degrees; casings should eject slightly to the right and behind of a gun
        _throw.TryThrow(entity, ejectAngle.ToVec().Normalized() * throwingForce, throwingSpeed);

        if (playSound && TryComp(entity, out CartridgeAmmoComponent? cartridge))
        {
            _audio.PlayPvs(cartridge.EjectSound, entity, AudioParams.Default.WithVariation(SharedContentAudioSystem.DefaultVariation).WithVolume(-1f));
        }
    }
}
