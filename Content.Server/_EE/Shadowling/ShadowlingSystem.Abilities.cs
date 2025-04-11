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
using Content.Shared.StatusEffect;
using Content.Shared.Strip.Components;
using Microsoft.CodeAnalysis.Operations;
using Microsoft.EntityFrameworkCore.Update;
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

        SubscribeLocalEvent<ShadowlingComponent, GlareEvent>(OnGlare);
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

        if (!CanEnthrall(uid, target))
            return;

        // Basic Enthrall -> Can't melt Mindshields
        if (HasComp<MindShieldComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("shadowling-enthrall-mindshield"), uid, uid, PopupType.SmallCaution);
            return;
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
        // todo:  raise local event in the future here
    }

    #endregion

    #region Glare

    private void OnGlare(EntityUid uid, ShadowlingComponent comp, GlareEvent args)
    {
        var target = args.Target;
        var user = args.Performer;

        if (!CanGlare(target))
            return;

        var targetCoords = _transform.GetWorldPosition(target);
        var distance = (_transform.GetWorldPosition(user) - targetCoords).Length();
        comp.GlareDistance = distance;
        comp.GlareTarget = target;

        // Glare mutes and slows down the target no matter what.
        if (TryComp<StatusEffectsComponent>(target, out var statComp))
        {
            _effects.TryAddStatusEffect(target, "Muted", TimeSpan.FromSeconds(comp.MuteTime), false, statComp);
            _stun.TrySlowdown(target, TimeSpan.FromSeconds(comp.SlowTime), false, 0.5f, 0.5f, statComp);
        }


        if (distance <= comp.MinGlareDistance)
        {
            comp.GlareStunTime = comp.MaxGlareStunTime;
            _stun.TryStun(target, TimeSpan.FromSeconds(comp.GlareStunTime), true);
        }
        else
        {
            // Do I know what is going on here? No. But it works so... Thanks for listening!
            comp.GlareStunTime = comp.MaxGlareStunTime * (1 - Math.Clamp(distance / comp.MaxGlareDistance, 0, 1));
            comp.GlareTimeBeforeEffect = comp.MinGlareDelay + (comp.MaxGlareDelay - comp.MinGlareDelay) * Math.Clamp(distance / comp.MaxGlareDistance, 0, 1);

            comp.ActivateGlareTimer = true;
        }

        _actions.StartUseDelay(args.Action);
    }

    #endregion
}
