using System.Linq;
using Content.Server.GameTicking;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Robust.Server.GameObjects;

namespace Content.Server.Punpun;

public sealed class PunpunSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly ServerMetaDataSystem _meta = default!;

    private (int, string, string) _punpunData = (0, string.Empty, string.Empty);

    // All the roman numerals we'll need to display
    private readonly Dictionary<int, string> _numerals = new()
    {
        { 0, "I" },
        { 1, "II" },
        { 2, "III" },
        { 3, "IV" },
        { 4, "V" },
        { 5, "VI" },
        { 6, "VII" },
        { 7, "VIII" },
        { 8, "IX" },
        { 9, "X" },
        { 10, "XI" },
        { 11, "XII" },
        { 12, "XIII" },
        { 13, "XIV" }
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PunpunComponent, ComponentStartup>(OnRoundStart);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEnd);
    }

    // Checks if the Punpun data has any items to equip, and names the Punpun upon initialization
    private void OnRoundStart(EntityUid uid, PunpunComponent component, ComponentStartup args)
    {
        if (_punpunData.Item1 > 13)
        {
            EntityManager.SpawnEntity("PaperWrittenPunpunNote", Transform(uid).Coordinates);
            EntityManager.QueueDeleteEntity(uid);
            _punpunData = (0, string.Empty, string.Empty);

            return;
        }

        var meta = MetaData(uid);
        _meta.SetEntityName(uid, $"{meta.EntityName} {_numerals[_punpunData.Item1]}", meta);

        if (!EntityManager.TryGetComponent<InventoryComponent>(uid, out _))
            return;
        EquipItem(uid, "head", _punpunData.Item2);
        EquipItem(uid, "mask", _punpunData.Item3);
    }

    // Checks if Punpun exists, and is alive at round end
    // If so, stores the items and increments the Punpun count
    private void OnRoundEnd(RoundEndTextAppendEvent ev)
    {
        // I couldn't find a method to get a single entity, so this just enumerated over the first and disposes it
        var punpunComponents = EntityManager.EntityQueryEnumerator<PunpunComponent>();
        punpunComponents.MoveNext(out var punpun, out _);

        if (!EntityManager.TryGetComponent<MobStateComponent>(punpun, out var mobState)
            || mobState.CurrentState == MobState.Dead)
            _punpunData = (0, string.Empty, string.Empty);

        _punpunData.Item1++;

        if (EntityManager.HasComponent<InventoryComponent>(punpun))
        {
            _punpunData.Item2 = CheckSlot(punpun, "head");
            _punpunData.Item3 = CheckSlot(punpun, "mask");
        }

        punpunComponents.Dispose();
    }

    // Equips an item to a slot, and names it.
    private void EquipItem(EntityUid uid, string slot, string item)
    {
        if (item == string.Empty)
            return;

        var itemEnt = EntityManager.SpawnEntity(item, EntityManager.GetComponent<TransformComponent>(uid).Coordinates);
        if (_inventory.TryEquip(uid, itemEnt, slot, true, true))
            _meta.SetEntityName(itemEnt, $"{MetaData(uid).EntityName}'s {MetaData(itemEnt).EntityName}");
        else
            EntityManager.DeleteEntity(itemEnt);
    }

    // Checks if an item exists in a slot, and returns its name
    private string CheckSlot(EntityUid uid, string slot)
    {
        return _inventory.TryGetSlotEntity(uid, slot, out var item)
            ? EntityManager.GetComponent<MetaDataComponent>(item.Value).EntityPrototype!.ID
            : string.Empty;
    }
}
