using System.Linq;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.Loadouts.Prototypes;
using Content.Shared.Customization.Systems;
using Content.Shared.Inventory;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Station;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Shared.Clothing.Loadouts.Systems;

public sealed class LoadoutSystem : EntitySystem
{
    [Dependency] private readonly SharedStationSpawningSystem _station = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly CharacterRequirementsSystem _characterRequirements = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LoadoutComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, LoadoutComponent component, MapInitEvent args)
    {
        if (component.Prototypes == null)
            return;

        var proto = _prototype.Index<StartingGearPrototype>(_random.Pick(component.Prototypes));
        _station.EquipStartingGear(uid, proto);
    }


    public List<EntityUid> ApplyCharacterLoadout(EntityUid uid, ProtoId<JobPrototype> job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes, bool whitelisted)
    {
        var jobPrototype = _prototype.Index(job);
        return ApplyCharacterLoadout(uid, jobPrototype, profile, playTimes, whitelisted);
    }

    /// <summary>
    ///     Equips entities from a <see cref="HumanoidCharacterProfile"/>'s loadout preferences to a given entity
    /// </summary>
    /// <param name="uid">The entity to give the loadout items to</param>
    /// <param name="job">The job to use for loadout whitelist/blacklist (should be the job of the entity)</param>
    /// <param name="profile">The profile to get loadout items from (should be the entity's, or at least have the same species as the entity)</param>
    /// <param name="playTimes">Playtime for the player for use with playtime requirements</param>
    /// <param name="whitelisted">If the player is whitelisted</param>
    /// <returns>A list of loadout items that couldn't be equipped but passed checks</returns>
    public List<EntityUid> ApplyCharacterLoadout(EntityUid uid, JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes, bool whitelisted)
    {
        var failedLoadouts = new List<EntityUid>();

        foreach (var loadout in profile.LoadoutPreferences)
        {
            var slot = "";

            // Ignore loadouts that don't exist
            if (!_prototype.TryIndex<LoadoutPrototype>(loadout, out var loadoutProto))
                continue;


            if (!_characterRequirements.CheckRequirementsValid(
                loadoutProto.Requirements, job, profile, playTimes, whitelisted, loadoutProto,
                EntityManager, _prototype, _configuration,
                out _))
                continue;


            // Spawn the loadout items
            var spawned = EntityManager.SpawnEntities(
                EntityManager.GetComponent<TransformComponent>(uid).Coordinates.ToMap(EntityManager),
                loadoutProto.Items.Select(p => (string?) p.ToString()).ToList()); // Dumb cast

            foreach (var item in spawned)
            {
                if (EntityManager.TryGetComponent<ClothingComponent>(item, out var clothingComp)
                    && _characterRequirements.CanEntityWearItem(uid, item)
                    && _inventory.TryGetSlots(uid, out var slotDefinitions))
                {
                    var deleted = false;
                    foreach (var curSlot in slotDefinitions)
                    {
                        // If the loadout can't equip here or we've already deleted an item from this slot, skip it
                        if (!clothingComp.Slots.HasFlag(curSlot.SlotFlags) || deleted)
                            continue;

                        slot = curSlot.Name;

                        // If the loadout is exclusive delete the equipped item
                        if (loadoutProto.Exclusive)
                        {
                            // Get the item in the slot
                            if (!_inventory.TryGetSlotEntity(uid, curSlot.Name, out var slotItem))
                                continue;

                            EntityManager.DeleteEntity(slotItem.Value);
                            deleted = true;
                        }
                    }
                }


                // Equip the loadout
                if (!_inventory.TryEquip(uid, item, slot, true, !string.IsNullOrEmpty(slot), true))
                    failedLoadouts.Add(item);
            }
        }

        // Return a list of items that couldn't be equipped so the server can handle it if it wants
        // The server has more information about the inventory system than the client does and the client doesn't need to put loadouts in backpacks
        return failedLoadouts;
    }
}
