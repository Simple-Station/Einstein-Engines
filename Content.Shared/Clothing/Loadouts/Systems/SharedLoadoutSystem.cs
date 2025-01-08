using System.Linq;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.Loadouts.Prototypes;
using Content.Shared.Customization.Systems;
using Content.Shared.Inventory;
using Content.Shared.Paint;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Station;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Serialization;

namespace Content.Shared.Clothing.Loadouts.Systems;

public sealed class SharedLoadoutSystem : EntitySystem
{
    [Dependency] private readonly SharedStationSpawningSystem _station = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly IConfigurationManager _configuration = default!;
    [Dependency] private readonly CharacterRequirementsSystem _characterRequirements = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedTransformSystem _sharedTransformSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LoadoutComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, LoadoutComponent component, MapInitEvent args)
    {
        if (component.StartingGear is null
            || component.StartingGear.Count <= 0)
            return;

        var proto = _prototype.Index(_random.Pick(component.StartingGear));
        _station.EquipStartingGear(uid, proto);
    }

    public (List<EntityUid>, List<(EntityUid, LoadoutPreference, int)>) ApplyCharacterLoadout(
        EntityUid uid,
        ProtoId<JobPrototype> job,
        HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        bool whitelisted,
        out List<(EntityUid, LoadoutPreference)> heirlooms)
    {
        var jobPrototype = _prototype.Index(job);
        return ApplyCharacterLoadout(uid, jobPrototype, profile, playTimes, whitelisted, out heirlooms);
    }

    /// <summary>
    ///     Equips entities from a <see cref="HumanoidCharacterProfile"/>'s loadout preferences to a given entity
    /// </summary>
    /// <param name="uid">The entity to give the loadout items to</param>
    /// <param name="job">The job to use for loadout whitelist/blacklist (should be the job of the entity)</param>
    /// <param name="profile">The profile to get loadout items from (should be the entity's, or at least have the same species as the entity)</param>
    /// <param name="playTimes">Playtime for the player for use with playtime requirements</param>
    /// <param name="whitelisted">If the player is whitelisted</param>
    /// <param name="heirlooms">Every entity the player selected as a potential heirloom</param>
    /// <returns>A list of loadout items that couldn't be equipped but passed checks</returns>
    public (List<EntityUid>, List<(EntityUid, LoadoutPreference, int)>) ApplyCharacterLoadout(
        EntityUid uid,
        JobPrototype job,
        HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        bool whitelisted,
        out List<(EntityUid, LoadoutPreference)> heirlooms)
    {
        var failedLoadouts = new List<EntityUid>();
        var allLoadouts = new List<(EntityUid, LoadoutPreference, int)>();
        heirlooms = new();

        foreach (var loadout in profile.LoadoutPreferences)
        {
            var slot = "";

            // Ignore loadouts that don't exist
            if (!_prototype.TryIndex<LoadoutPrototype>(loadout.LoadoutName, out var loadoutProto))
                continue;

            if (!_characterRequirements.CheckRequirementsValid(
                loadoutProto.Requirements, job, profile, playTimes, whitelisted, loadoutProto,
                EntityManager, _prototype, _configuration,
                out _))
                continue;

            // Spawn the loadout items
            var spawned = EntityManager.SpawnEntities(
                _sharedTransformSystem.GetMapCoordinates(uid),
                loadoutProto.Items.Select(p => (string?) p.ToString()).ToList()); // Dumb cast

            var i = 0; // If someone wants to add multi-item support to the editor
            foreach (var item in spawned)
            {
                allLoadouts.Add((item, loadout, i));
                if (loadout.CustomHeirloom == true)
                    heirlooms.Add((item, loadout));

                // Equip it
                if (EntityManager.TryGetComponent<ClothingComponent>(item, out var clothingComp)
                    && _characterRequirements.CanEntityWearItem(uid, item, true)
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

                // Color it
                if (loadout.CustomColorTint != null)
                {
                    EnsureComp<AppearanceComponent>(item);
                    EnsureComp<PaintedComponent>(item, out var paint);
                    paint.Color = Color.FromHex(loadout.CustomColorTint);
                    paint.Enabled = true;
                    _appearance.TryGetData(item, PaintVisuals.Painted, out bool data);
                    _appearance.SetData(item, PaintVisuals.Painted, !data);
                }

                // Equip the loadout
                if (!_inventory.TryEquip(uid, item, slot, true, !string.IsNullOrEmpty(slot), true))
                    failedLoadouts.Add(item);

                i++;
            }
        }

        // Return a list of items that couldn't be equipped so the server can handle it if it wants
        // The server has more information about the inventory system than the client does and the client doesn't need to put loadouts in backpacks
        return (failedLoadouts, allLoadouts);
    }
}

[Serializable, NetSerializable, ImplicitDataDefinitionForInheritors]
public abstract partial class Loadout
{
    [DataField] public string LoadoutName { get; set; }
    [DataField] public string? CustomName { get; set; }
    [DataField] public string? CustomDescription { get; set; }
    [DataField] public string? CustomColorTint { get; set; }
    [DataField] public bool? CustomHeirloom { get; set; }

    protected Loadout(
        string loadoutName,
        string? customName = null,
        string? customDescription = null,
        string? customColorTint = null,
        bool? customHeirloom = null
    )
    {
        LoadoutName = loadoutName;
        CustomName = customName;
        CustomDescription = customDescription;
        CustomColorTint = customColorTint;
        CustomHeirloom = customHeirloom;
    }
}

[Serializable, NetSerializable]
public sealed partial class LoadoutPreference : Loadout
{
    [DataField] public bool Selected;

    public LoadoutPreference(
        string loadoutName,
        string? customName = null,
        string? customDescription = null,
        string? customColorTint = null,
        bool? customHeirloom = null
    ) : base(loadoutName, customName, customDescription, customColorTint, customHeirloom) { }
}
