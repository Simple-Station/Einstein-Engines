// /Content.Server/Vehicles/Systems/VehicleKeySystem.cs
using Content.Shared.Vehicles.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.IoC;

namespace Content.Server.Vehicles.Systems
{
    public sealed class VehicleKeySystem : EntitySystem
    {
        public override void Initialize()
        {
            base.Initialize();
            SubscribeLocalEvent<VehicleKeyComponent, ComponentStartup>(OnComponentStartup);
            SubscribeLocalEvent<VehicleKeyComponent, ComponentShutdown>(OnComponentShutdown);
        }

        private void OnComponentStartup(EntityUid uid, VehicleKeyComponent component, ComponentStartup args)
        {
            //  logic for vehicle keys
        }

        private void OnComponentShutdown(EntityUid uid, VehicleKeyComponent component, ComponentShutdown args)
        {
            // Cleanup logic for vehicle keys (e.g., removing the key from the vehicle ? I dont fucking know man clown car needs car keys)
        }

        public bool IsVehicleLocked(EntityUid uid)
        {
            return TryComp<VehicleKeyComponent>(uid, out var keyComp) && keyComp.IsLocked;
        }

        public void LockVehicle(EntityUid uid)
        {
            if (TryComp<VehicleKeyComponent>(uid, out var keyComp))
            {
                keyComp.IsLocked = true;
            }
        }

        public void UnlockVehicle(EntityUid uid)
        {
            if (TryComp<VehicleKeyComponent>(uid, out var keyComp))
            {
                keyComp.IsLocked = false;
            }
        }
    }
}
