using Content.Shared.Emag.Systems;
using Content.Shared.Examine;
using Content.Shared.Lock;
using Content.Shared.Popups;
using System.Linq;

namespace Content.Shared._White.Lockers;

public abstract class SharedStationAlertLevelLockSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationAlertLevelLockComponent, LockToggleAttemptEvent>(OnLockToggleAttempt);
        SubscribeLocalEvent<StationAlertLevelLockComponent, GotEmaggedEvent>(OnEmagged);
        SubscribeLocalEvent<StationAlertLevelLockComponent, ExaminedEvent>(OnExamined);
    }

    private void OnLockToggleAttempt(Entity<StationAlertLevelLockComponent> ent, ref LockToggleAttemptEvent args)
    {
        if (!TryComp<LockComponent>(ent, out var lockComp))
            return;

        var locking = !lockComp.Locked; // Allow locking even if the alert level is wrong
        if (!ent.Comp.Enabled || !ent.Comp.Locked || locking)
            return;

        _popup.PopupClient(Loc.GetString("access-failed-wrong-station-alert-level"), ent.Owner, args.User);

        args.Cancelled = true;
    }

    private void OnEmagged(Entity<StationAlertLevelLockComponent> ent, ref GotEmaggedEvent args)
    {
        // don't waste multiple emag charges
        if (!ent.Comp.Enabled)
            return;

        args.Handled = true;
        ent.Comp.Enabled = false;
        Dirty(ent);
    }

    private void OnExamined(Entity<StationAlertLevelLockComponent> ent, ref ExaminedEvent args)
    {
        if (!ent.Comp.Enabled || ent.Comp.LockedAlertLevels.Count == 0)
            return;

        var levels = string.Join(", ", ent.Comp.LockedAlertLevels.Select( s => Loc.GetString($"alert-level-{s}").ToLower()));

        args.PushMarkup(Loc.GetString("station-alert-level-lock-examined", ("levels", levels)));
    }

    protected void CheckAlertLevels(Entity<StationAlertLevelLockComponent> ent, string newAlertLevel)
    {
        ent.Comp.Locked = ent.Comp.LockedAlertLevels.Contains(newAlertLevel);
        Dirty(ent);
    }
}
