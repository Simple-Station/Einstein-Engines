using System.Linq;
using Content.Shared.Backmen.ModSuits.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Clothing;
using Content.Shared.Wires;

namespace Content.Shared.Backmen.ModSuits.Systems;


public sealed class ModSuitModSystem : EntitySystem
{
    [Dependency] private readonly ModSuitSystem _modsuit = default!;
    [Dependency] private readonly ClothingSpeedModifierSystem _speedModifier = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ModSuitModComponent, ItemSlotInsertAttemptEvent>(OnInsert);
        SubscribeLocalEvent<ModSuitModComponent, ItemSlotEjectAttemptEvent>(OnEject);
    }

    private void OnInsert(EntityUid uid, ModSuitModComponent component, ref ItemSlotInsertAttemptEvent args)
    {
        if (args.Cancelled || component.Inserted)
            return;

        if (!TryComp<WiresPanelComponent>(args.SlotEntity, out var panel) || !panel.Open)
        {
            args.Cancelled = true;
            return;
        }

        if (!TryComp<ModSuitComponent>(args.SlotEntity, out var modsuit) || _modsuit.GetAttachedToggleStatus(args.SlotEntity, modsuit) != ModSuitAttachedStatus.NoneToggled)
        {
            args.Cancelled = true;
            return;
        }

        var container = modsuit.Container;
        if (container == null)
            return;

        if (TryComp<ClothingSpeedModifierComponent>(args.SlotEntity, out var modify))
        {
            _speedModifier.ModifySpeed(uid, modify, component.SpeedMod);

            component.Inserted = true;
        }

        var attachedClothings = modsuit.ClothingUids;
        if (component.Slot == "MODcore")
        {
            EntityManager.AddComponents(args.SlotEntity, component.Components);
            component.Inserted = true;
            return;
        }

        foreach (var attached in attachedClothings
                     .Where(attached => container.Contains(attached.Key))
                     .Where(attached => attached.Value == component.Slot))
        {
            EntityManager.AddComponents(attached.Key, component.Components);
            if (component.RemoveComponents != null)
            {
                EntityManager.RemoveComponents(attached.Key, component.RemoveComponents);
            }

            break;
        }
    }
    private void OnEject(EntityUid uid, ModSuitModComponent component, ref ItemSlotEjectAttemptEvent args)
    {
        if (!TryComp<WiresPanelComponent>(args.SlotEntity, out var panel) || !panel.Open)
        {
            args.Cancelled = true;
            return;
        }

        if (args.Cancelled || !component.Inserted)
            return;

        if (!TryComp<ModSuitComponent>(args.SlotEntity, out var modsuit)
            || _modsuit.GetAttachedToggleStatus(args.SlotEntity, modsuit) != ModSuitAttachedStatus.NoneToggled)
        {
            args.Cancelled = true;
            return;
        }

        var container = modsuit.Container;
        if (container == null)
            return;

        if (TryComp<ClothingSpeedModifierComponent>(args.SlotEntity, out var modify))
        {
            _speedModifier.ModifySpeed(uid, modify, -component.SpeedMod);

            component.Inserted = false;
        }

        var attachedClothings = modsuit.ClothingUids;
        if (component.Slot == "MODcore")
        {
            EntityManager.RemoveComponents(args.SlotEntity, component.Components);
            component.Inserted = false;
            return;
        }

        foreach (var attached in attachedClothings.Where(attached => container.Contains(attached.Key)).Where(attached => attached.Value == component.Slot))
        {
            EntityManager.RemoveComponents(attached.Key, component.Components);
            if (component.RemoveComponents != null)
            {
                EntityManager.AddComponents(attached.Key, component.RemoveComponents);
            }

            break;
        }
    }
}
