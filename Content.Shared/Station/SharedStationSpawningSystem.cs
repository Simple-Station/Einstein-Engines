using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared.Dataset;
using Content.Shared.Customization.Systems;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Preferences;
using Content.Shared.Preferences.Loadouts;
using Content.Shared.Roles;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Collections;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Shared.Station;

public abstract class SharedStationSpawningSystem : EntitySystem
{
    [Dependency] protected readonly IPrototypeManager PrototypeManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] protected readonly InventorySystem InventorySystem = default!;
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly SharedStorageSystem _storage = default!;
    [Dependency] private readonly SharedTransformSystem _xformSystem = default!;
    [Dependency] private readonly MetaDataSystem _metadata = default!;
    [Dependency] private readonly IConfigurationManager _configurationManager = default!;
    [Dependency] private readonly CharacterRequirementsSystem _characterRequirements = default!;

    private EntityQuery<HandsComponent> _handsQuery;
    private EntityQuery<InventoryComponent> _inventoryQuery;
    private EntityQuery<StorageComponent> _storageQuery;
    private EntityQuery<TransformComponent> _xformQuery;

    public override void Initialize()
    {
        base.Initialize();
        _handsQuery = GetEntityQuery<HandsComponent>();
        _inventoryQuery = GetEntityQuery<InventoryComponent>();
        _storageQuery = GetEntityQuery<StorageComponent>();
        _xformQuery = GetEntityQuery<TransformComponent>();
    }

    /// <summary>
    /// Applies the role's name as applicable to the entity.
    /// </summary>
    public void EquipJobName(EntityUid entity, JobPrototype job)
    {
        string? name = null;

        if (string.IsNullOrEmpty(name)
            && job.NameDataset.HasValue
            && PrototypeManager.TryIndex(job.NameDataset.Value, out var nameData))
        {
            name = Loc.GetString(_random.Pick(nameData.Values));
        }

        if (!string.IsNullOrEmpty(name))
        {
            _metadata.SetEntityName(entity, name);
        }
    }

    /// <summary>
    /// <see cref="EquipStartingGear(Robust.Shared.GameObjects.EntityUid,System.Nullable{Robust.Shared.Prototypes.ProtoId{Content.Shared.Roles.StartingGearPrototype}},bool)"/>
    /// </summary>
    public void EquipStartingGear(EntityUid entity, ProtoId<StartingGearPrototype>? startingGear, bool raiseEvent = true)
    {
        PrototypeManager.TryIndex(startingGear, out var gearProto);
        EquipStartingGear(entity, gearProto);
    }

    /// <summary>
    /// Equips starting gear onto the given entity.
    /// </summary>
    /// <param name="entity">Entity to load out.</param>
    /// <param name="startingGear">Starting gear to use.</param>
    /// <param name="raiseEvent">Should we raise the event for equipped. Set to false if you will call this manually</param>
    public void EquipStartingGear(EntityUid entity, StartingGearPrototype? startingGear, bool raiseEvent = true)
    {
        if (startingGear == null)
            return;

        if (GetProfile(entity, out var profile))
            // Equip any sub-gears of this starting gear.
            startingGear = ApplySubGear(startingGear, profile);

        var xform = _xformQuery.GetComponent(entity);

        if (InventorySystem.TryGetSlots(entity, out var slotDefinitions))
        {
            foreach (var slot in slotDefinitions)
            {
                var equipmentStr = startingGear.GetGear(slot.Name, null);
                if (string.IsNullOrEmpty(equipmentStr))
                    continue;

                var equipmentEntity = EntityManager.SpawnEntity(equipmentStr, xform.Coordinates);
                InventorySystem.TryEquip(entity, equipmentEntity, slot.Name, true, force:true);
            }
        }

        if (_handsQuery.TryComp(entity, out var handsComponent))
        {
            var inhand = startingGear.Inhand;
            var coords = xform.Coordinates;
            foreach (var prototype in inhand)
            {
                var inhandEntity = EntityManager.SpawnEntity(prototype, coords);

                if (_handsSystem.TryGetEmptyHand(entity, out var emptyHand, handsComponent))
                {
                    _handsSystem.TryPickup(entity, inhandEntity, emptyHand, checkActionBlocker: false,
                        handsComp: handsComponent);
                }
            }
        }

        if (startingGear.Storage.Count > 0)
        {
            var coords = _xformSystem.GetMapCoordinates(entity);
            var ents = new ValueList<EntityUid>();
            _inventoryQuery.TryComp(entity, out var inventoryComp);

            foreach (var (slot, entProtos) in startingGear.Storage)
            {
                if (entProtos.Count == 0)
                    continue;

                foreach (var ent in entProtos)
                {
                    ents.Add(Spawn(ent, coords));
                }

                if (inventoryComp != null &&
                    InventorySystem.TryGetSlotEntity(entity, slot, out var slotEnt, inventoryComponent: inventoryComp) &&
                    _storageQuery.TryComp(slotEnt, out var storage))
                {
                    foreach (var ent in ents)
                    {
                        _storage.Insert(slotEnt.Value, ent, out _, storageComp: storage, playSound: false);
                    }
                }
            }
        }

        if (raiseEvent)
        {
            var ev = new StartingGearEquippedEvent(entity);
            RaiseLocalEvent(entity, ref ev);
        }
    }

    public bool GetProfile(EntityUid? uid, [NotNullWhen(true)] out HumanoidCharacterProfile? profile)
    {
        if (!TryComp(uid, out HumanoidAppearanceComponent? appearance))
        {
            profile = null;
            return false;
        }

        if (appearance.LastProfileLoaded is { } lastProfileLoaded)
        {
            profile = lastProfileLoaded;
            return true;
        }

        profile = HumanoidCharacterProfile.DefaultWithSpecies(appearance.Species);
        return true;
    }

    // <summary>
    //   Apply a starting gear's sub-gears to itself, returning a new starting gear prototype with
    //   replaced equipment.
    // </summary>
    public StartingGearPrototype ApplySubGear(StartingGearPrototype startingGear, HumanoidCharacterProfile profile, JobPrototype? job = null)
    {
        if (startingGear.SubGears.Count == 0)
            return startingGear;

        // Job can be null for cases like ghost roles' starting gear which do not have a job definition.
        job ??= new JobPrototype();

        var newStartingGear = startingGear;
        var foundConditionalMatch = false;

        foreach (var subGear in startingGear.SubGears)
        {
            if (!PrototypeManager.TryIndex<StartingGearPrototype>(subGear.Id, out var subGearProto) ||
                !_characterRequirements.CheckRequirementsValid(
                    subGearProto.Requirements, job, profile, new Dictionary<string, TimeSpan>(), false, job,
                    EntityManager, PrototypeManager, _configurationManager,
                    out _))
                continue;

            // Apply the sub-gear's sub-gears if there are any
            subGearProto = ApplySubGear(subGearProto, profile, job);

            if (!foundConditionalMatch)
            {
                foundConditionalMatch = true;
                // Lazy init on making a new starting gear prototype for performance reasons.
                // We can't just modify the original prototype or it will be modified for everyone.
                newStartingGear = new StartingGearPrototype()
                {
                    Equipment = startingGear.Equipment.ToDictionary(static entry => entry.Key, static entry => entry.Value),
                    InnerClothingSkirt = startingGear.InnerClothingSkirt,
                    Satchel = startingGear.Satchel,
                    Duffelbag = startingGear.Duffelbag,
                    Inhand = new List<EntProtoId>(startingGear.Inhand),
                    Storage = startingGear.Storage.ToDictionary(
                        static entry => entry.Key,
                        static entry => new List<EntProtoId>(entry.Value)
                    ),
                };
            }

            // Apply the sub-gear's equipment to this starting gear
            if (subGearProto.InnerClothingSkirt != null)
                newStartingGear.InnerClothingSkirt = subGearProto.InnerClothingSkirt;

            if (subGearProto.Satchel != null)
                newStartingGear.Satchel = subGearProto.Satchel;

            if (subGearProto.Duffelbag != null)
                newStartingGear.Duffelbag = subGearProto.Duffelbag;

            foreach (var (slot, entProtoId) in subGearProto.Equipment)
            {
                // Don't remove items in pockets, instead put them in the backpack or hands
                if (slot == "pocket1" && newStartingGear.Equipment.TryGetValue("pocket1", out var pocket1) ||
                    slot == "pocket2" && newStartingGear.Equipment.TryGetValue("pocket2", out var pocket2))
                {
                    var pocketProtoId = slot == "pocket1" ? pocket1 : pocket2;

                    if (string.IsNullOrEmpty(newStartingGear.GetGear("back", null)))
                        newStartingGear.Inhand.Add(pocketProtoId);
                    else
                    {
                        if (!newStartingGear.Storage.ContainsKey("back"))
                            newStartingGear.Storage["back"] = new();
                        newStartingGear.Storage["back"].Add(pocketProtoId);
                    }
                }

                newStartingGear.Equipment[slot] = entProtoId;
            }

            newStartingGear.Inhand.AddRange(subGearProto.Inhand);

            foreach (var (slot, entProtoIds) in subGearProto.Storage)
                newStartingGear.Storage[slot].AddRange(entProtoIds);
        }

        return newStartingGear;
    }
}
