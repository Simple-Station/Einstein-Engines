using Content.Server.AlertLevel;
using Content.Server.Station.Systems;
using Content.Shared._White.Lockers;

namespace Content.Server._White.Lockers;

public sealed class StationAlertLevelLockSystem : SharedStationAlertLevelLockSystem
{
    [Dependency] private readonly AlertLevelSystem _level = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StationAlertLevelLockComponent, MapInitEvent>(OnInit);
        SubscribeLocalEvent<AlertLevelChangedEvent>(OnAlertChanged);
    }

    public void OnInit(Entity<StationAlertLevelLockComponent> ent, ref MapInitEvent args)
    {
        // for non-station mapped safes don't lock them because that's chuddy
        if (_station.GetOwningStation(ent.Owner) is not {} station)
        {
            ent.Comp.Enabled = false;
            Dirty(ent);
            return;
        }

        ent.Comp.StationId = station;
        ent.Comp.Enabled = true;

        CheckAlertLevels(ent, _level.GetLevel(station));
        Dirty(ent);
    }

    private void OnAlertChanged(AlertLevelChangedEvent args)
    {
        var query = EntityQueryEnumerator<StationAlertLevelLockComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            var station = args.Station;
            if (station != comp.StationId)
                continue;

            CheckAlertLevels((uid, comp), args.AlertLevel);
        }
    }
}
