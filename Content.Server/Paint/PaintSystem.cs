using Content.Shared.Popups;
using Content.Shared.Paint;
using Content.Shared.Sprite;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Server.Chemistry.Containers.EntitySystems;
using Robust.Shared.Audio.Systems;
using Content.Shared.Humanoid;
using Robust.Shared.Utility;
using Content.Shared.Verbs;
using Content.Shared.SubFloor;
using Content.Server.Nutrition.Components;
using Content.Shared.Inventory;
using Content.Server.Nutrition.EntitySystems;
using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Whitelist;
using Robust.Shared.Audio;

namespace Content.Server.Paint;

/// <summary>
/// Colors target and consumes reagent on each color success.
/// </summary>
public sealed class PaintSystem : SharedPaintSystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SolutionContainerSystem _solutionContainer = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly OpenableSystem _openable = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PaintComponent, AfterInteractEvent>(OnInteract);
        SubscribeLocalEvent<PaintComponent, PaintDoAfterEvent>(OnPaint);
        SubscribeLocalEvent<PaintComponent, GetVerbsEvent<UtilityVerb>>(OnPaintVerb);
    }


    private void OnInteract(EntityUid uid, PaintComponent component, AfterInteractEvent args)
    {
        if (!args.CanReach
            || args.Target is not { Valid: true } target)
            return;

        PrepPaint(uid, component, target, args.User);
    }

    private void OnPaintVerb(EntityUid uid, PaintComponent component, GetVerbsEvent<UtilityVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess)
            return;

        var paintText = Loc.GetString("paint-verb");

        var verb = new UtilityVerb()
        {
            Act = () =>
            {
                PrepPaint(uid, component, args.Target, args.User);
            },

            Text = paintText,
            Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/VerbIcons/paint.svg.192dpi.png"))
        };
        args.Verbs.Add(verb);
    }

    private void PrepPaint(EntityUid uid, PaintComponent component, EntityUid target, EntityUid user) =>
        _doAfterSystem.TryStartDoAfter(
            new DoAfterArgs(
                EntityManager,
                user,
                component.Delay,
                new PaintDoAfterEvent(),
                uid,
                target: target,
                used: uid)
            {
                BreakOnUserMove = true,
                BreakOnTargetMove = true,
                NeedHand = true,
                BreakOnHandChange = true,
            });

    private void OnPaint(Entity<PaintComponent> entity, ref PaintDoAfterEvent args)
    {
        if (args.Target == null || args.Used == null || args.Handled || args.Cancelled || args.Target is not { Valid: true } target)
            return;

        Paint(entity, target, args.User);
        args.Handled = true;
    }

    public void Paint(Entity<PaintComponent> entity, EntityUid target, EntityUid user)
    {
        if (!_openable.IsOpen(entity))
        {
            _popup.PopupEntity(Loc.GetString("paint-closed", ("used", entity)), user, user, PopupType.Medium);
            return;
        }

        if (HasComp<PaintedComponent>(target) || HasComp<RandomSpriteComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("paint-failure-painted", ("target", target)), user, user, PopupType.Medium);
            return;
        }

        if (!entity.Comp.Whitelist?.IsValid(target, EntityManager) == true
            || !entity.Comp.Blacklist?.IsValid(target, EntityManager) == false
            || HasComp<HumanoidAppearanceComponent>(target) || HasComp<SubFloorHideComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("paint-failure", ("target", target)), user, user, PopupType.Medium);
            return;
        }

        if (CanPaint(entity, target))
        {
            EnsureComp<PaintedComponent>(target, out var paint);
            EnsureComp<AppearanceComponent>(target);

            paint.Color = entity.Comp.Color;
            _audio.PlayPvs(entity.Comp.Spray, entity);
            paint.Enabled = true;

            // Paint any clothing the target is wearing
            if (HasComp<InventoryComponent>(target)
                && _inventory.TryGetSlots(target, out var slotDefinitions))
                foreach (var slot in slotDefinitions)
                {
                    if (!_inventory.TryGetSlotEntity(target, slot.Name, out var slotEnt)
                        || HasComp<PaintedComponent>(slotEnt.Value)
                        || !entity.Comp.Whitelist?.IsValid(slotEnt.Value, EntityManager) != true
                        || !entity.Comp.Blacklist?.IsValid(slotEnt.Value, EntityManager) != false
                        || HasComp<RandomSpriteComponent>(slotEnt.Value)
                        || HasComp<HumanoidAppearanceComponent>(slotEnt.Value))
                        continue;

                    EnsureComp<PaintedComponent>(slotEnt.Value, out var slotToPaint);
                    EnsureComp<AppearanceComponent>(slotEnt.Value);
                    slotToPaint.Color = entity.Comp.Color;
                    _appearanceSystem.SetData(slotEnt.Value, PaintVisuals.Painted, true);
                    Dirty(slotEnt.Value, slotToPaint);
                }

            _popup.PopupEntity(Loc.GetString("paint-success", ("target", target)), user, user, PopupType.Medium);
            _appearanceSystem.SetData(target, PaintVisuals.Painted, true);
            Dirty(target, paint);
            return;
        }

        if (!CanPaint(entity, target))
            _popup.PopupEntity(Loc.GetString("paint-empty", ("used", entity)), user, user, PopupType.Medium);
    }

    public void Paint(EntityWhitelist? whitelist, EntityWhitelist? blacklist, EntityUid target, Color color)
    {
        if (!whitelist?.IsValid(target, EntityManager) != true
            || !blacklist?.IsValid(target, EntityManager) != false)
            return;

        EnsureComp<PaintedComponent>(target, out var paint);
        EnsureComp<AppearanceComponent>(target);

        paint.Color = color;
        paint.Enabled = true;

        if (HasComp<InventoryComponent>(target)
            && _inventory.TryGetSlots(target, out var slotDefinitions))
            foreach (var slot in slotDefinitions)
            {
                if (!_inventory.TryGetSlotEntity(target, slot.Name, out var slotEnt)
                    || !whitelist?.IsValid(slotEnt.Value, EntityManager) != true
                    || !blacklist?.IsValid(slotEnt.Value, EntityManager) != false)
                    continue;

                EnsureComp<PaintedComponent>(slotEnt.Value, out var slotToPaint);
                EnsureComp<AppearanceComponent>(slotEnt.Value);
                slotToPaint.Color = color;
                _appearanceSystem.SetData(slotEnt.Value, PaintVisuals.Painted, true);
                Dirty(slotEnt.Value, slotToPaint);
            }

        _appearanceSystem.SetData(target, PaintVisuals.Painted, true);
        Dirty(target, paint);
    }

    private bool CanPaint(Entity<PaintComponent> reagent, EntityUid target)
    {
        if (HasComp<HumanoidAppearanceComponent>(target)
            || HasComp<SubFloorHideComponent>(target)
            || !_solutionContainer.TryGetSolution(reagent.Owner, reagent.Comp.Solution, out _, out var solution))
            return false;
        var quantity = solution.RemoveReagent(reagent.Comp.Reagent, reagent.Comp.ConsumptionUnit);
        return (quantity > 0);
    }
}
