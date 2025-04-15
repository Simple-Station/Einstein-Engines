using Content.Server.Power.Components;
using Content.Server.Shuttles.Components;
using Content.Server.Station.Systems;

namespace Content.Server._Crescent.Station;

/// <summary>
/// Handles the figurative "life and death" of vessels for gameplay purposes.
/// </summary>
public sealed partial class StationLifeSystem : EntitySystem
{
    [Dependency] private readonly StationSystem _station = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StationLifeHeuristicComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<StationLifeHeuristicComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnPowerChanged(EntityUid uid, StationLifeHeuristicComponent component, PowerChangedEvent args)
    {
        var station = _station.GetOwningStation(uid);

        if (station != null)
            UpdateStationLife(station.Value);
    }

    private void OnShutdown(EntityUid uid, StationLifeHeuristicComponent component, ComponentShutdown args)
    {
        var station = _station.GetOwningStation(uid);

        if (station != null)
            UpdateStationLife(station.Value);
    }

    private void UpdateStationLife(EntityUid uid)
    {
        var ev = new StationLifeCheckEvent();
        RaiseLocalEvent(uid, ref ev);
    }

    /// <summary>
    /// Whether the station that owns this entity is alive.
    /// </summary>
    public bool IsAlive(EntityUid uid)
    {
        var station = _station.GetOwningStation(uid);

        if (station == null)
            return false;

        // A good starting heuristic: Does the station contain a powered shuttle console?
        bool poweredConsole = false;

        var consoles = EntityQueryEnumerator<StationLifeHeuristicComponent, ApcPowerReceiverComponent>();

        while (consoles.MoveNext(out var consoleUid, out var _, out var receiver))
        {
            if (_station.GetOwningStation(consoleUid) != station)
                continue;

            if (receiver.Powered)
            {
                poweredConsole = true;
                break;
            }
        }

        return poweredConsole;
    }
}

[ByRefEvent]
public record struct StationLifeCheckEvent;
