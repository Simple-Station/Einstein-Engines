using Content.Shared.Weapons.Ranged.Events;
using Content.Shared._Goobstation.Weapons.SmartGun;
using Content.Shared.Weapons; // for ShotAttemptedEvent (or OnShotAttemptedEvent in your fork)


namespace Content.Server._Goobstation.Weapons.SmartGun
{
    /// <summary>
    /// Handles SmartGun behavior for entities with SmartGunUserComponent.
    /// </summary>
    public sealed class SmartGunUserSystem : EntitySystem
    {
        [Dependency] private readonly IEntityManager _entMan = default!;

        public override void Initialize()
        {
            base.Initialize();

            // Listen for firing events on entities with the SmartGunUserComponent
            SubscribeLocalEvent<SmartGunUserComponent, ShotAttemptedEvent>(OnShotAttempted);
        }

        private void OnShotAttempted(EntityUid uid, SmartGunUserComponent comp, ShotAttemptedEvent args)
        {
            Logger.Info($"SmartGunUser {uid} attempted to fire a smartgun.");
        }
    }

}
// TODO: Implement smartgun targeting/tracking logic here

