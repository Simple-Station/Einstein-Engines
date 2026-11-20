/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CE.ZLevels.Flight.Components;
using Content.Shared.DoAfter;
using Content.Shared.Toggleable;

namespace Content.Shared._CE.ZLevels.Flight;

public abstract partial class CESharedZFlightSystem
{
    private void InitializeControllable()
    {
        SubscribeLocalEvent<CEControllableFlightComponent, CEZFlightActionUp>(OnZLevelUp);
        SubscribeLocalEvent<CEControllableFlightComponent, CEZFlightActionDown>(OnZLevelDown);
        SubscribeLocalEvent<CEControllableFlightComponent, ToggleActionEvent>(OnZLevelToggle);

        SubscribeLocalEvent<CEControllableFlightComponent, CEStartFlightDoAfterEvent>(OnStartFlightDoAfter);
        SubscribeLocalEvent<CEControllableFlightComponent, CEFlightStartedEvent>(OnControllableFlightStarted);
        SubscribeLocalEvent<CEControllableFlightComponent, CEFlightStoppedEvent>(OnControllableFlightStopped);
    }

    private void OnControllableFlightStopped(Entity<CEControllableFlightComponent> ent, ref CEFlightStoppedEvent args)
    {
        _actions.SetEnabled(ent.Comp.ZLevelDownActionEntity, false);
        _actions.SetEnabled(ent.Comp.ZLevelUpActionEntity, false);

        // Update toggle action icon state
        if (ent.Comp.ZLevelToggleActionEntity != null)
            _actions.SetToggled(ent.Comp.ZLevelToggleActionEntity, false);
    }

    private void OnControllableFlightStarted(Entity<CEControllableFlightComponent> ent, ref CEFlightStartedEvent args)
    {
        _actions.SetEnabled(ent.Comp.ZLevelDownActionEntity, true);
        _actions.SetEnabled(ent.Comp.ZLevelUpActionEntity, true);

        // Update toggle action icon state
        if (ent.Comp.ZLevelToggleActionEntity != null)
            _actions.SetToggled(ent.Comp.ZLevelToggleActionEntity, true);
    }

    private void OnZLevelUp(Entity<CEControllableFlightComponent> ent, ref CEZFlightActionUp args)
    {
        if (args.Handled)
            return;

        var map = Transform(ent).MapUid;
        if (map is null)
            return;

        if (!TryComp<CEZFlyerComponent>(ent, out var flyerComp))
            return;

        if (!_zLevel.TryMapUp(map.Value, out var mapAbove))
            return;

        flyerComp.TargetMapHeight = mapAbove.Value.Comp.Depth;
        DirtyField(ent, flyerComp, nameof(CEZFlyerComponent.TargetMapHeight));

        args.Handled = true;
    }

    private void OnZLevelDown(Entity<CEControllableFlightComponent> ent, ref CEZFlightActionDown args)
    {
        if (args.Handled)
            return;

        var map = Transform(ent).MapUid;
        if (map is null)
            return;

        if (!TryComp<CEZFlyerComponent>(ent, out var flyerComp))
            return;

        if (!_zLevel.TryMapDown(map.Value, out var mapBelow))
            return;

        flyerComp.TargetMapHeight = mapBelow.Value.Comp.Depth;
        DirtyField(ent, flyerComp, nameof(CEZFlyerComponent.TargetMapHeight));

        args.Handled = true;
    }

    private void OnZLevelToggle(Entity<CEControllableFlightComponent> ent, ref ToggleActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<CEZFlyerComponent>(ent, out var flyerComp))
            return;

        if (flyerComp.Active)
        {
            DeactivateFlight((ent, flyerComp));
        }
        else
        {
            // If StartFlightDoAfter is set, start a doAfter before activating flight
            if (ent.Comp.StartFlightDoAfter != null)
            {
                //Preventive start flying visuals
                StartFlightVisuals((ent, flyerComp));

                var doAfter = new DoAfterArgs(EntityManager, ent, ent.Comp.StartFlightDoAfter.Value, new CEStartFlightDoAfterEvent(), ent)
                {
                    BreakOnMove = false,
                    BlockDuplicate = true,
                    BreakOnDamage = true,
                    CancelDuplicate = true,
                };

                _doAfter.TryStartDoAfter(doAfter);
            }
            else
            {
                // No delay, activate flight immediately
                TryActivateFlight((ent, flyerComp));
            }
        }

        args.Handled = true;
    }

    private void OnStartFlightDoAfter(Entity<CEControllableFlightComponent> ent, ref CEStartFlightDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
        {
            StopFlightVisuals(ent.Owner);
            return;
        }

        TryActivateFlight(ent.Owner);
        args.Handled = true;
    }
}
