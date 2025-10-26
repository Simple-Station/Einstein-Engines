using Content.Shared.ArachnidChaos;
using Content.Shared.Armor;
using Content.Shared.Chasm;
using Content.Shared.Chat;
using Content.Shared.Clothing;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Content.Shared.Tools;
using Content.Shared.Tools.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._Crescent.DegradeableArmor;

[Serializable, NetSerializable]
public enum ArmorRepairMaterial
{
    PlasteelPlate = 1 << 0,
    NTPolymer = 1 << 1,
    CeramicPlate = 1 << 2,
    SteelPlate = 1 << 3,
    DuraThread = 1 << 4,
    PlasmaGlass = 1 << 5,
    Plastic = 1 << 6,
    HomelandAlloy = 1 << 7,
    Kevlar = 1 << 8,
    PlasteelEncasedKevlar = 1 << 9,
    NTCeramic = 1 << 10

}
[Serializable, NetSerializable]
public partial class ArmorRepairDoAfterEvent : SimpleDoAfterEvent
{

}
/// <summary>
/// This handles...
/// </summary>
public sealed class DegradeableArmorSystem : EntitySystem
{
    [Dependency] private readonly StaminaSystem _stamina = default!;
    [Dependency] private readonly DamageableSystem _damage = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedDoAfterSystem _doing = default!;
    [Dependency] private readonly ClothingSystem _cloth = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    [Dependency] private readonly SharedToolSystem _toolSystem = default!;

    private const string conversionPrototype = "PiercingInducedBlunt";
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<DegradeableArmorComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<DegradeableArmorComponent, InventoryRelayedEvent<DamageModifyEvent>>(OnDamageModify);
        SubscribeLocalEvent<DegradeableArmorComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<DegradeableArmorComponent, ClothingGotEquippedEvent>(afterEquipped);
        SubscribeLocalEvent<DegradeableArmorComponent, ClothingGotUnequippedEvent>(afterDeequip);
        SubscribeLocalEvent<DegradeableArmorComponent, ArmorRepairDoAfterEvent>(OnRepair);
        SubscribeLocalEvent<DegradeableArmorComponent, ExaminedEvent>(OnArmorExamine);
    }

    private void OnInteractUsing(Entity<DegradeableArmorComponent> owner, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;
        if (owner.Comp.armorHealth == owner.Comp.armorMaxHealth)
        {
            _popup.PopupEntity("This armor piece is not damaged!", owner.Owner, args.User, PopupType.Medium);
            return;
        }

        if (_inventory.TryGetContainingSlot(owner.Owner, out var def))
        {
            _popup.PopupClient("You can't repair this piece whilst wearing it!", args.User, args.User, PopupType.Medium);
            return;
        }

        args.Handled = _toolSystem.UseTool(args.Used, args.User, owner.Owner, 15, "Welding", new ArmorRepairDoAfterEvent(), 50);

    }

    private void OnRepair(Entity<DegradeableArmorComponent> owner, ref ArmorRepairDoAfterEvent args)
    {
        if(args.Cancelled)
            return;

        owner.Comp.armorHealth = owner.Comp.armorMaxHealth;
        if (TryComp<ToggleableClothingComponent>(owner.Owner, out var component))
        {
            foreach (var (ClothingUid, _) in component.ClothingUids)
            {
                if (TryComp<DegradeableArmorComponent>(ClothingUid, out var headwearArmor))
                {
                    headwearArmor.armorHealth = headwearArmor.armorMaxHealth;
                }
            }

        }
    }
    private void afterEquipped(EntityUid owner, DegradeableArmorComponent comp, ref ClothingGotEquippedEvent args)
    {
        comp.wearer = args.Wearer;
    }

    private void afterDeequip(EntityUid owner, DegradeableArmorComponent comp, ref ClothingGotUnequippedEvent args)
    {
        comp.wearer = EntityUid.Invalid;
    }
    private void OnArmorExamine(EntityUid owner, DegradeableArmorComponent component, ref ExaminedEvent args)
    {
        args.PushMessage(GetArmorExamine(component));
    }
    private FormattedMessage GetArmorExamine(DegradeableArmorComponent component)
    {
        var msg = new FormattedMessage();

        msg.AddMarkup(Loc.GetString("armor-examine"));

        foreach (var flatArmor in component.initialModifiers.FlatReduction)
        {
            msg.PushNewline();

            var armorType = Loc.GetString("armor-damage-type-" + flatArmor.Key.ToLower());
            msg.AddMarkup(Loc.GetString("armor-reduction-value",
                ("type", armorType),
                ("value", (int)(flatArmor.Value * (component.armorHealth+000.1f)/component.armorMaxHealth))
            ));
        }

        return msg;
    }
    private void OnInit(EntityUid uid, DegradeableArmorComponent component, ref MapInitEvent args)
    {
        if(component.armorHealth == 0)
            component.armorHealth = component.armorMaxHealth;
    }
    private void OnDamageModify(EntityUid uid, DegradeableArmorComponent component, InventoryRelayedEvent<DamageModifyEvent> args)
    {
        if (component.armorHealth <= 0)
            return;
        var armorDamage = 0f;


        var damageDictionary = args.Args.Damage.DamageDict;
        damageDictionary.TryAdd(conversionPrototype, 0);
        foreach (var (type, value) in damageDictionary)
        {
            if (!component.initialModifiers.FlatReduction.ContainsKey(type))
                continue;
            if (value < 0)
                continue;
            var trueReduction = component.initialModifiers.FlatReduction[type];
            if (trueReduction == 0)
                continue;
            switch (component.armorType)
            {
                // Ceramic armor has internal energy and it mitigates the impact of the bullet force-wise
                case ArmorDegradation.Ceramic:
                {
                    trueReduction *= component.armorHealth / component.armorMaxHealth;
                    trueReduction *= component.armorHealth / component.armorMaxHealth;
                    _stamina.TakeStaminaDamage(component.wearer, args.Args.stoppingPower);
                    break;
                }
                // Spreads damage internally
                case ArmorDegradation.Metallic:
                {
                    trueReduction *= component.armorHealth / component.armorMaxHealth;
                    damageDictionary[conversionPrototype] += args.Args.stoppingPower;
                    break;
                }
                case ArmorDegradation.Plastic:
                {
                    trueReduction *= (component.armorHealth + (float) value * 2) / component.armorMaxHealth;
                    _stamina.TakeStaminaDamage(component.wearer, args.Args.stoppingPower);
                    break;
                }
            }

            trueReduction = Math.Clamp(trueReduction - args.Args.HullrotArmorPen, 0f, (float) value);
            armorDamage += (float) value * component.armorDamageCoefficients[type];
            //Logger.Error($"Damage adjusted for type {type}, old {value}, new {Math.Max(0f, (float) value - trueReduction)}  Armor damage {armorDamage}. Armor Health {component.armorHealth}. Stamina damage {trueReduction * component.staminaConversions[type]}");
            damageDictionary[type] = Math.Max(0f, (float) value - trueReduction);
        }
        component.armorHealth = Math.Max(0, component.armorHealth - armorDamage);
        Dirty(uid, component);
    }

}
