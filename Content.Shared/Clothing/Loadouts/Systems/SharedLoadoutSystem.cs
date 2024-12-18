using System.Linq;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.Loadouts.Prototypes;
using Content.Shared.Customization.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Inventory;
using Content.Shared.Paint;
using Content.Shared.Preferences;
using Content.Shared.Prototypes;
using Content.Shared.Roles;
using Content.Shared.Station;
using Content.Shared.Traits.Assorted.Components;
using Robust.Shared.Configuration;
using Robust.Shared.Log;
using Robust.Shared.Network;
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
    [Dependency] private readonly INetManager _net = default!;

    [Dependency] private readonly SharedTransformSystem _sharedTransformSystem = default!;

    private List<CharacterItemGroupPrototype> _groupProtosWithMinItems = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LoadoutComponent, MapInitEvent>(OnMapInit);

        _groupProtosWithMinItems = _prototype.EnumeratePrototypes<CharacterItemGroupPrototype>()
                .Where(g => g.MinItems > 0).ToList();
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

        var preferencesAndPrototypes = DetermineItems(uid, job, profile, playTimes, whitelisted);

        foreach (var (loadout, loadoutProto) in preferencesAndPrototypes)
        {
            // Spawn the loadout items
            var spawned = EntityManager.SpawnEntities(
                _sharedTransformSystem.GetMapCoordinates(uid),
                loadoutProto.Items.Select(p => (string?) p.ToString()).ToList()); // Dumb cast

            var i = 0; // If someone wants to add multi-item support to the editor
            foreach (var item in spawned)
            {
                var slots = new List<String>();

                allLoadouts.Add((item, loadout, i));
                if (loadout.CustomHeirloom == true)
                    heirlooms.Add((item, loadout));

                // Equip it
                if (EntityManager.TryGetComponent<ClothingComponent>(item, out var clothingComp)
                    && _characterRequirements.CanEntityWearItem(uid, item)
                    && _inventory.TryGetSlots(uid, out var slotDefinitions))
                {
                    foreach (var curSlot in slotDefinitions)
                    {
                        // If the loadout can't equip here, skip it
                        if (!clothingComp.Slots.HasFlag(curSlot.SlotFlags))
                            continue;

                        slots.Add(curSlot.Name);

                        // If the loadout is exclusive delete the equipped item
                        if (loadoutProto.Exclusive)
                        {
                            // Get the item in the slot
                            if (!_inventory.TryGetSlotEntity(uid, curSlot.Name, out var slotItem))
                                continue;

                            EntityManager.DeleteEntity(slotItem.Value);
                            break;
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
                var equipped = false;
                foreach (var slot in slots)
                {
                    if (_inventory.TryEquip(uid, item, slot, true, !string.IsNullOrEmpty(slot), true))
                    {
                        equipped = true;
                        break;
                    }
                }

                if (!equipped)
                    failedLoadouts.Add(item);

                i++;
            }
        }

        var ev = new StartingGearEquippedEvent(uid);
        RaiseLocalEvent(uid, ref ev);

        // Return a list of items that couldn't be equipped so the server can handle it if it wants
        // The server has more information about the inventory system than the client does and the client doesn't need to put loadouts in backpacks
        return (failedLoadouts, allLoadouts);
    }

    /// <summary>
    ///   Returns a list with all items validated using the user's profile,
    ///   adding items based on MinItems and removing items based on MaxItems from <see cref="CharacterItemGroupPrototype"/>.
    /// </summary>
    private List<(LoadoutPreference, LoadoutPrototype)> DetermineItems(
        EntityUid uid,
        JobPrototype job,
        HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        bool whitelisted)
    {
        var groupIDToGroup = new Dictionary<String, CharacterItemGroupPrototype>();
        var groupIDToLoadoutIDToItem = new Dictionary<String, Dictionary<String, CharacterItemGroupItem>>();
        var groupIDToGroupItems = new Dictionary<String, HashSet<CharacterItemGroupItem>>();

        var loadoutIDToPreferenceAndProto = new Dictionary<String, (LoadoutPreference, LoadoutPrototype)>();

        foreach (var (loadoutID, loadout) in profile.LoadoutPreferences.ToDictionary(l => l.LoadoutName, l => l))
        {
            if (!_prototype.TryIndex<LoadoutPrototype>(loadout.LoadoutName, out var loadoutProto) ||
                !_characterRequirements.CheckRequirementsValid(
                    loadoutProto.Requirements, job, profile, playTimes, whitelisted, loadoutProto,
                    EntityManager, _prototype, _configuration,
                    out _))
                continue;

            var skip = false;
            var requirementsSucceeded = loadoutProto.Groups.Count == 0;

            foreach (var groupID in loadoutProto.Groups)
            {
                if (!_prototype.TryIndex<CharacterItemGroupPrototype>(groupID, out var groupProto))
                    continue;

                // Right now you only need one group requirement to pass to be eligible for the item
                if (groupIDToGroup.ContainsKey(groupID))
                {
                    requirementsSucceeded = true;
                }
                else
                {
                    if (_characterRequirements.CheckRequirementsValid(
                        groupProto.Requirements, job, profile, playTimes, whitelisted, groupProto,
                        EntityManager, _prototype, _configuration,
                        out _))
                        requirementsSucceeded = true;

                    groupIDToGroup[groupID] = groupProto;
                    groupIDToLoadoutIDToItem[groupID] = groupProto.Items.ToDictionary(i => i.ID, i => i);
                    groupIDToGroupItems[groupID] = new HashSet<CharacterItemGroupItem>();
                }

                if (!groupIDToLoadoutIDToItem[groupID].TryGetValue(loadoutProto.ID, out var groupItem))
                {
                    Log.Error($"Expected loadout item '{loadoutProto.ID}' to be part of the items in the prototype of group '{groupID}'");
                    skip = true;
                }
                else
                {
                    groupIDToGroupItems[groupID].Add(groupItem);
                }
            }

            if (!requirementsSucceeded || skip)
                continue;

            loadoutIDToPreferenceAndProto[loadoutID] = (loadout, loadoutProto);
        }

        // Add groups with minimum items
        foreach (var groupProto in _groupProtosWithMinItems)
        {
            if (groupIDToGroup.ContainsKey(groupProto.ID) ||
                !_characterRequirements.CheckRequirementsValid(
                groupProto.Requirements, job, profile, playTimes, whitelisted, groupProto,
                EntityManager, _prototype, _configuration, out _))
                continue;

            groupIDToGroup[groupProto.ID] = groupProto;
            groupIDToLoadoutIDToItem[groupProto.ID] = groupProto.Items.ToDictionary(i => i.ID, i => i);
            groupIDToGroupItems[groupProto.ID] = new HashSet<CharacterItemGroupItem>();
        }

        if (groupIDToGroup.Count == 0)
            return loadoutIDToPreferenceAndProto.Values.ToList();

        // Use a clone for the foreach loop since we are modifying the original
        var originalGroupIDToGroupItems = new Dictionary<String, HashSet<CharacterItemGroupItem>>(groupIDToGroupItems);

        // Start modifying the loadout preferences based on MinItems/MaxItems
        foreach (var (groupID, items) in originalGroupIDToGroupItems)
        {
            if (!groupIDToGroup.TryGetValue(groupID, out var groupProto))
                continue;

            var count = items.Count;
            // Remove items, prioritizing removing the higher priority items first
            if (count > groupProto.MaxItems)
            {
                var sortedItems = items.OrderByDescending(i => i.Priority).ThenBy(i => i.ID);

                foreach (var itemToRemove in sortedItems)
                {
                    loadoutIDToPreferenceAndProto.Remove(itemToRemove.ID);
                    count--;

                    if (count <= groupProto.MaxItems)
                        break;
                }
            }
            // For each loadout group that doesn't have enough items, add until MinItems is satisfied
            else if (count < groupProto.MinItems)
            {
                foreach (var itemToAdd in groupProto.Items)
                {
                    if (!_prototype.TryIndex<LoadoutPrototype>(itemToAdd.ID, out var loadoutProto))
                        continue;

                    if (!_characterRequirements.CheckRequirementsValid(
                        loadoutProto.Requirements, job, profile, playTimes, whitelisted, loadoutProto,
                        EntityManager, _prototype, _configuration,
                        out _))
                        continue;

                    var skip = false;
                    var requirementsSucceeded = false;

                    foreach (var otherGroupID in loadoutProto.Groups)
                    {
                        if (groupIDToGroup.ContainsKey(otherGroupID))
                        {
                            requirementsSucceeded = true;
                        }
                        else
                        {
                            if (!_prototype.TryIndex<CharacterItemGroupPrototype>(otherGroupID, out var otherGroupProto))
                                continue;

                            if (_characterRequirements.CheckRequirementsValid(
                                otherGroupProto.Requirements, job, profile, playTimes, whitelisted, otherGroupProto,
                                EntityManager, _prototype, _configuration,
                                out _))
                                requirementsSucceeded = true;

                            groupIDToGroup[otherGroupID] = otherGroupProto;
                            groupIDToLoadoutIDToItem[otherGroupID] = otherGroupProto.Items.ToDictionary(i => i.ID, i => i);
                            groupIDToGroupItems[otherGroupID] = new HashSet<CharacterItemGroupItem>();
                        }

                        if (!groupIDToLoadoutIDToItem[otherGroupID].TryGetValue(loadoutProto.ID, out var _))
                        {
                            Log.Error($"Expected loadout item '{loadoutProto.ID}' to be part of the items in the prototype of group '{otherGroupID}'");
                            skip = true;
                            continue;
                        }

                        // If adding this item would make other groups have more than the maximum items, don't add it
                        if (groupIDToGroupItems[otherGroupID].Count + 1 > groupIDToGroup[otherGroupID].MaxItems)
                            skip = true;
                    }

                    if (!requirementsSucceeded || skip)
                        continue;

                    // Add this item to other groups for the sake of counting
                    foreach (var otherGroupID in loadoutProto.Groups)
                    {
                        groupIDToGroupItems[otherGroupID].Add(groupIDToLoadoutIDToItem[otherGroupID][loadoutProto.ID]);
                    }

                    loadoutIDToPreferenceAndProto[itemToAdd.ID] = (new LoadoutPreference(itemToAdd.ID), loadoutProto);
                    count++;

                    if (count >= groupProto.MaxItems)
                        break;
                }
            }
        }

        return loadoutIDToPreferenceAndProto.Values.ToList();
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
