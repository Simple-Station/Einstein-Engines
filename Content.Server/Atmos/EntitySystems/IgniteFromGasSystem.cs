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

        /// <summary>
        ///   These are the inventory slot groups that are checked for ignition immunity.
        ///   If no slot in a group has IgniteFromGasImmunityComponent, no immunity is applied.
        /// </summary>
        [DataField]
        private List<List<String>> ImmunitySlotGroups = new() {
            new () { "head" },
            new () { "jumpsuit", "outerClothing" }
        };

        public override void Initialize()
        {
            SubscribeLocalEvent<IgniteFromGasImmunityComponent, GotEquippedEvent>(OnIgniteFromGasImmunityEquipped);
            SubscribeLocalEvent<IgniteFromGasImmunityComponent, GotUnequippedEvent>(OnIgniteFromGasImmunityUnequipped);
        }

        private void OnIgniteFromGasImmunityEquipped(EntityUid uid, IgniteFromGasImmunityComponent igniteImmunity, GotEquippedEvent args)
        {
            if (TryComp<IgniteFromGasComponent>(args.Equipee, out var ignite) && ImmunitySlotGroups.Any(group => group.Contains(args.Slot)))
            {
                ignite.HasImmunity = HasIgniteImmunity(args.Equipee);
            }
        }

        private void OnIgniteFromGasImmunityUnequipped(EntityUid uid, IgniteFromGasImmunityComponent igniteImmunity, GotUnequippedEvent args)
        {
            if (TryComp<IgniteFromGasComponent>(args.Equipee, out var ignite) && ImmunitySlotGroups.Any(group => group.Contains(args.Slot)))
            {
                ignite.HasImmunity = HasIgniteImmunity(args.Equipee);
            }
        }

        public bool HasIgniteImmunity(EntityUid uid, InventoryComponent? inv = null, ContainerManagerComponent? contMan = null)
        {
            if (!Resolve(uid, ref inv, ref contMan))
                return false;

            foreach (var group in ImmunitySlotGroups)
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
                    return false;
                }
            }

            return true;
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
