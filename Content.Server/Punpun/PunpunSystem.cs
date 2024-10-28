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

    private (int, string, string, string) _punpunData = (1, string.Empty, string.Empty, string.Empty);


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PunpunComponent, ComponentStartup>(OnRoundStart);
        SubscribeLocalEvent<RoundEndTextAppendEvent>(OnRoundEnd);
    }


    // Checks if the Punpun data has any items to equip, and names the Punpun upon initialization
    private void OnRoundStart(EntityUid uid, PunpunComponent component, ComponentStartup args)
    {
        if (_punpunData.Item1 > component.Lifetime)
        {
            EntityManager.SpawnEntity("PaperWrittenPunpunNote", Transform(uid).Coordinates);
            EntityManager.QueueDeleteEntity(uid);
            _punpunData = (1, string.Empty, string.Empty, string.Empty);

            return;
        }

        var meta = MetaData(uid);
        _meta.SetEntityName(uid, $"{meta.EntityName} {ToRomanNumeral(_punpunData.Item1)}", meta);

        if (!EntityManager.TryGetComponent<InventoryComponent>(uid, out _))
            return;
        EquipItem(uid, "head", _punpunData.Item2);
        EquipItem(uid, "mask", _punpunData.Item3);
        EquipItem(uid, "jumpsuit", _punpunData.Item4);
    }

    // Checks if Punpun exists, and is alive at round end
    // If so, stores the items and increments the Punpun count
    private void OnRoundEnd(RoundEndTextAppendEvent ev)
    {
        // I couldn't find a method to get a single entity, so this just enumerates over the first and disposes it
        var punpunComponents = EntityManager.EntityQueryEnumerator<PunpunComponent>();
        punpunComponents.MoveNext(out var punpun, out _);

        if (!EntityManager.TryGetComponent<MobStateComponent>(punpun, out var mobState)
            || mobState.CurrentState == MobState.Dead)
            _punpunData = (1, string.Empty, string.Empty, string.Empty);

        _punpunData.Item1++;

        if (EntityManager.HasComponent<InventoryComponent>(punpun))
        {
            _punpunData.Item2 = CheckSlot(punpun, "head");
            _punpunData.Item3 = CheckSlot(punpun, "mask");
            _punpunData.Item4 = CheckSlot(punpun, "jumpsuit");
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


    // Punpun, the lord of Roman Numerals
    public static List<string> RomanNumerals = new() { "M",  "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX", "V", "IV", "I" };
    public static List<int> Numerals = new()         { 1000, 900,  500, 400,  100, 90,   50,  40,   10,  9,    5,   4,    1   };

    public static string ToRomanNumeral(int number)
    {
        var romanNumeral = string.Empty;
        while (number > 0)
        {
            // Find the biggest numeral that is less than equal to number
            var index = Numerals.FindIndex(x => x <= number);
            // Subtract its value from your number
            number -= Numerals[index];
            // Add it onto the end of your roman numeral
            romanNumeral += RomanNumerals[index];
        }
        return romanNumeral;
    }
}
