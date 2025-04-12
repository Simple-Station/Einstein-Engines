using Content.Server.Storage.Components;
using Content.Shared._EE.Clothing.Components;
using Content.Shared._EE.Shadowling;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles...
/// </summary>
public sealed partial class ShadowlingSystem
{
    public void SubscribeAbilities()
    {
        SubscribeLocalEvent<ShadowlingComponent, HatchEvent>(OnHatch);
    }

    # region Hatch

    private void OnHatch(EntityUid uid, ShadowlingComponent comp, HatchEvent args)
    {
        _actions.RemoveAction(uid, args.Action);

        StartHatchingProgress(uid, comp);

        comp.CurrentPhase = ShadowlingPhases.PostHatch;

        AddPostHatchActions(uid, comp);
    }

    private void StartHatchingProgress(EntityUid uid, ShadowlingComponent comp)
    {
        comp.IsHatching = true;

        // Shadowlings change skin colour once hatched
        if (TryComp<HumanoidAppearanceComponent>(uid, out var appearance))
        {
            appearance.SkinColor = comp.SkinColor;

            // Respect the markings
            foreach (var (_, listMarkings) in appearance.MarkingSet.Markings)
            {
                foreach (var marking in listMarkings)
                    marking.SetColor(comp.SkinColor);
            }

            appearance.EyeColor = comp.EyeColor;
            Dirty(uid, appearance);
        }

        // Drop all items
        if (TryComp<InventoryComponent>(uid, out var inv))
        {
            foreach (var slot in inv.Slots)
                _inventorySystem.DropSlotContents(uid, slot.Name, inv);
        }

        // Shadowlings can't wear any clothes
        EnsureComp<ShadowlingCannotWearClothesComponent>(uid);

        var egg = SpawnAtPosition(comp.Egg, Transform(uid).Coordinates);
        if (TryComp<HatchingEggComponent>(egg, out var eggComp) &&
            TryComp<EntityStorageComponent>(egg, out var eggStorage))
        {
            eggComp.ShadowlingInside = uid;
            _entityStorage.Insert(uid, egg, eggStorage);
        }

        // It should be noted that Shadowling shouldn't be able to take damage during this process.
    }

    #endregion
}
