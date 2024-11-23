using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Server.Administration.Logs;
using Content.Server.Atmos.Components;
using Content.Shared.Alert;
using Content.Shared.Atmos;
using Content.Shared.Damage;
using Content.Shared.Database;
using Content.Shared.FixedPoint;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Mood;
using Robust.Shared.Containers;

namespace Content.Server.Atmos.EntitySystems
{
    public sealed class IgniteFromGasSystem : EntitySystem
    {
        [Dependency] private readonly AtmosphereSystem _atmos = default!;
        [Dependency] private readonly FlammableSystem _flammable = default!;
        [Dependency] private readonly AlertsSystem _alerts = default!;
        [Dependency] private readonly IAdminLogManager _adminLogger= default!;
        [Dependency] private readonly InventorySystem _inventory = default!;

        private const float UpdateTimer = 1f;
        private float _timer;

        public override void Initialize()
        {
            SubscribeLocalEvent<IgniteFromGasImmunityComponent, GotEquippedEvent>(OnIgniteFromGasImmunityEquipped);
            SubscribeLocalEvent<IgniteFromGasImmunityComponent, GotUnequippedEvent>(OnIgniteFromGasImmunityUnequipped);
        }

        private void OnIgniteFromGasImmunityEquipped(EntityUid uid, IgniteFromGasImmunityComponent igniteImmunity, GotEquippedEvent args)
        {
            if (TryComp<IgniteFromGasComponent>(args.Equipee, out var ignite) && ignite.ImmunitySlotGroups.Any(group => group.Contains(args.Slot)))
            {
                UpdateImmunity(args.Equipee, ignite);
            }
        }

        private void OnIgniteFromGasImmunityUnequipped(EntityUid uid, IgniteFromGasImmunityComponent igniteImmunity, GotUnequippedEvent args)
        {
            if (TryComp<IgniteFromGasComponent>(args.Equipee, out var ignite) && ignite.ImmunitySlotGroups.Any(group => group.Contains(args.Slot)))
            {
                UpdateImmunity(args.Equipee, ignite);
            }
        }

        private void UpdateImmunity(EntityUid uid, IgniteFromGasComponent ignite)
        {
            if (ignite.ImmunitySlotGroups.Count == 0 ||
                !TryComp(uid, out InventoryComponent? inv) ||
                !TryComp(uid, out ContainerManagerComponent? contMan))
                return;

            ignite.HasImmunity = true;

            foreach (var group in ignite.ImmunitySlotGroups)
            {
                var groupHasImmunity = false;

                foreach (var slot in group)
                {
                    if (_inventory.TryGetSlotEntity(uid, slot, out var equipment, inv, contMan) &&
                        HasComp<IgniteFromGasImmunityComponent>(equipment))
                    {
                        groupHasImmunity = true;
                        break;
                    }
                }

                if (!groupHasImmunity)
                {
                    ignite.HasImmunity = false;
                    return;
                }
            }
        }

        public override void Update(float frameTime)
        {
            _timer += frameTime;

            if (_timer < UpdateTimer)
                return;

            _timer -= UpdateTimer;

            var enumerator = EntityQueryEnumerator<IgniteFromGasComponent, FlammableComponent>();
            while (enumerator.MoveNext(out var uid, out var ignite, out var flammable))
            {
                if (ignite.HasImmunity)
                    continue;

                var gas = _atmos.GetContainingMixture(uid, excite: true);

                if (gas is null)
                    continue;

                if (gas[(int) ignite.Gas] > ignite.MolesToIgnite)
                {
                    _flammable.AdjustFireStacks(uid, ignite.FireStacks, flammable);
                    _flammable.Ignite(uid, uid, flammable);
                }
            }
        }
    }
}
