// SPDX-License-Identifier: AGPL-3.0-or-later
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Wieldable.Components;
using Content.Shared._Shitcode.Wizard.Projectiles;
using Content.Shared.Popups; //  import popup system

namespace Content.Shared._Goobstation.Weapons.SmartGun;

public sealed class SmartGunSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SmartGunComponent, AmmoShotEvent>(OnShot);
        SubscribeLocalEvent<SmartGunComponent, ShotAttemptedEvent>(OnShotAttempted);
    }

    private void OnShot(Entity<SmartGunComponent> ent, ref AmmoShotEvent args)
    {
        var (uid, comp) = ent;

        if (!TryComp(uid, out GunComponent? gun) || gun.Target == null)
            return;

        if (comp.RequiresWield && !(TryComp(uid, out WieldableComponent? wieldable) && wieldable.Wielded))
            return;

        foreach (var projectile in args.FiredProjectiles)
        {
            if (!TryComp(projectile, out HomingProjectileComponent? homing))
                continue;

            homing.Target = gun.Target.Value;
            Dirty(projectile, homing);
        }
    }

    private void OnShotAttempted(EntityUid uid, SmartGunComponent comp, ref ShotAttemptedEvent args)
    {
        if (!HasComp<SmartGunUserComponent>(args.User))
        {

            //_popup.PopupEntity("UNAUTHORIZED WIELDER, RETURN PROPERTY TO LAWFUL OWNER", args.User, args.User);
            args.Cancel(); // cancels the shot
            

        }
    }
}

