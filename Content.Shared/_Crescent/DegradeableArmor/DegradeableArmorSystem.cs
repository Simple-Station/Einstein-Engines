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
using Robust.Shared.Containers;
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
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedDoAfterSystem _doing = default!;
    [Dependency] private readonly ClothingSystem _cloth = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<DegradeableArmorComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<DegradeableArmorComponent, InventoryRelayedEvent<DamageModifyEvent>>(OnDamageModify);
        SubscribeLocalEvent<DegradeableArmorComponent, ClothingGotEquippedEvent>(afterEquipped);
        SubscribeLocalEvent<DegradeableArmorComponent, ClothingGotUnequippedEvent>(afterDeequip);
        SubscribeLocalEvent<ArmorRepairKitComponent, AfterInteractEvent>(OnArmorKitUse);
        SubscribeLocalEvent<ArmorRepairKitComponent, ArmorRepairDoAfterEvent>(OnArmorDoAfter);
        SubscribeLocalEvent<DegradeableArmorComponent, ExaminedEvent>(OnArmorExamine);
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

    private void OnArmorDoAfter(EntityUid uid, ArmorRepairKitComponent component, ref ArmorRepairDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;
        if (!TryComp<DegradeableArmorComponent>(args.Target, out var targetComponent))
            return;
        if (targetComponent.armorHealth >= targetComponent.armorMaxHealth)
        {
            _popup.PopupClient("This is already in pristine condition!", args.User, args.User, PopupType.Medium);
            return;
        }

        if (_inventory.TryGetContainingSlot(args.Target.Value, out var def))
        {
            _popup.PopupClient("You can't repair this piece whilst wearing it!", args.User, args.User, PopupType.Medium);
            return;
        }
        var usedArmor = Math.Min(component.repairHealth, targetComponent.armorMaxHealth - targetComponent.armorHealth);
        component.repairHealth -= usedArmor;
        if (component.repairHealth <= 0)
        {
            QueueDel(uid);
        }

        targetComponent.armorHealth += usedArmor;
        _popup.PopupClient(
            $"You use the armor kit. The armor on the target is now at {100 * (FixedPoint2) targetComponent.armorHealth / targetComponent.armorMaxHealth}% health",
            args.User, args.User, PopupType.Medium);
    }
    private void OnArmorKitUse(EntityUid uid, ArmorRepairKitComponent component, ref AfterInteractEvent args)
    {
        if (!TryComp<DegradeableArmorComponent>(args.Target, out var targetComponent))
            return;
        if (targetComponent.armorRepair != component.materialType)
        {
            _popup.PopupClient("You can't use this material to repair this!", args.User, args.User, PopupType.Medium);
            return;
        }

        if (targetComponent.armorHealth >= targetComponent.armorMaxHealth)
        {
            _popup.PopupClient("This is already in pristine condition!", args.User, args.User, PopupType.Medium);
            return;
        }

        if (_inventory.TryGetContainingSlot(args.Target.Value, out var def))
        {
            _popup.PopupClient("You can't repair this piece whilst wearing it!",args.User,args.User, PopupType.Medium);
            return;
        }
        var doAfterEventArgs = new DoAfterArgs(EntityManager, args.User, 5f, new ArmorRepairDoAfterEvent(), uid, target: args.Target, used: uid)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            BlockDuplicate = true
        };
        _doing.TryStartDoAfter(doAfterEventArgs);


    }


    private void OnInit(EntityUid uid, DegradeableArmorComponent component, ref MapInitEvent args)
    {
        if(component.armorHealth == 0)
            component.armorHealth = component.armorMaxHealth;
    }
    private void OnDamageModify(EntityUid uid, DegradeableArmorComponent component, InventoryRelayedEvent<DamageModifyEvent> args)
    {
        if (component.armorHealth == 0)
            return;
        var armorDamage = 0f;


        var damageDictionary = args.Args.Damage.DamageDict;
        foreach (var (type, value) in damageDictionary)
        {
            if (!component.initialModifiers.FlatReduction.ContainsKey(type))
                continue;

            var trueReduction = component.initialModifiers.FlatReduction[type];
            if (trueReduction == 0)
                continue;
            switch (component.armorType)
            {
                case ArmorDegradation.Ceramic:
                {
                    trueReduction *= component.armorHealth / component.armorMaxHealth;
                    trueReduction *= component.armorHealth / component.armorMaxHealth;

                    break;
                }
                case ArmorDegradation.Metallic:
                {
                    trueReduction *= component.armorHealth / component.armorMaxHealth;
                    break;
                }
                case ArmorDegradation.Plastic:
                {
                    trueReduction *= (component.armorHealth + (float) value * 2) / component.armorMaxHealth;
                    break;
                }
            }

            trueReduction = Math.Clamp(trueReduction, 0f, component.maxBlockCoefficients[type] * (float) value);
            _stamina.TakeStaminaDamage(component.wearer, trueReduction * component.staminaConversions[type]);
            armorDamage += (float) value * component.armorDamageCoefficients[type];
            //Logger.Error($"Damage adjusted for type {type}, old {value}, new {Math.Max(0f, (float) value - trueReduction)}  Armor damage {armorDamage}. Armor Health {component.armorHealth}. Stamina damage {trueReduction * component.staminaConversions[type]}");
            damageDictionary[type] = Math.Max(0f, (float) value - trueReduction);
        }

        component.armorHealth = Math.Max(0, component.armorHealth - armorDamage);
        Dirty(uid, component);
    }

}
