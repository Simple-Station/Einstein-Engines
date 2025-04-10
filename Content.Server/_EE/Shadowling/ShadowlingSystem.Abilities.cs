using System.Numerics;
using Content.Server.EntityEffects.EffectConditions;
using Content.Server.EntityEffects.Effects;
using Content.Server.Storage.Components;
using Content.Shared._EE.Clothing.Components;
using Content.Shared._EE.Clothing.Systems;
using Content.Shared._EE.Shadowling;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Clothing.Components;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.Electrocution;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Inventory;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Singularity;
using Content.Shared.Strip.Components;
using Microsoft.CodeAnalysis.Operations;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles...
/// </summary>
public sealed partial class ShadowlingSystem
{
    public void SubscribeAbilities()
    {
        SubscribeLocalEvent<ShadowlingComponent, HatchEvent>(OnHatch);

        SubscribeLocalEvent<ShadowlingComponent, EnthrallEvent>(OnEnthrall);
        SubscribeLocalEvent<ShadowlingComponent, EnthrallDoAfterEvent>(OnEnthrallDoAfter);
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

    #region Enthrall

    private void OnEnthrall(EntityUid uid, ShadowlingComponent comp, EnthrallEvent args)
    {
        var target = args.Target;
        var doAfterArgs = new DoAfterArgs(
            EntityManager,
            uid,
            comp.EnthrallTime,
            new EnthrallDoAfterEvent(),
            uid,
            target);

        #region Popups

        if (HasComp<ShadowlingComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("shadowling-enthrall-shadowling"), uid, uid, PopupType.SmallCaution);
            return;
        }

        if (HasComp<ThrallComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("shadowling-enthrall-already-thrall"), uid, uid, PopupType.SmallCaution);
            return;
        }

        if (HasComp<MindShieldComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("shadowling-enthrall-mindshield"), uid, uid, PopupType.SmallCaution);
            return;
        }

        if (!HasComp<HumanoidAppearanceComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("shadowling-enthrall-non-humanoid"), uid, uid, PopupType.SmallCaution);
            return;
        }

        // Psionic interaction
        if (HasComp<PsionicInsulationComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("shadowling-enthrall-psionic-insulated"), uid, uid, PopupType.SmallCaution);
            return;
        }

        // Target needs to be alive
        if (TryComp<MobStateComponent>(target, out var mobState))
        {
            if (_mobStateSystem.IsCritical(target, mobState) || _mobStateSystem.IsCritical(target, mobState))
            {
                _popup.PopupEntity(Loc.GetString("shadowling-enthrall-dead"), uid, uid, PopupType.SmallCaution);
                return;
            }
        }

        _popup.PopupEntity(Loc.GetString("shadowling-target-being-thralled"), uid, target, PopupType.SmallCaution);

        #endregion

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnEnthrallDoAfter(EntityUid uid, ShadowlingComponent comp, EnthrallDoAfterEvent args)
    {
        if (args.Cancelled)
            return;
        if (args.Args.Target is null)
            return;

        var target = args.Args.Target.Value;

        EnsureComp<ThrallComponent>(target);
        comp.Thralls.Add(target);
    }

    #endregion
}
