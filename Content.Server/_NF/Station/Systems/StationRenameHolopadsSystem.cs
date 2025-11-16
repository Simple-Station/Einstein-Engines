using Content.Server.Labels;
using Content.Server.Station.Components;
using Content.Server.Station.Events;
using Content.Server.Station.Systems;
using Content.Shared.Holopad;

namespace Content.Server._NF.Station.Systems;

public sealed class StationRenameHolopadsSystem : EntitySystem
{
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly LabelSystem _label = default!; // TODO: use LabelSystem directly instead of this.

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StationRenameHolopadsComponent, StationPostInitEvent>(OnPostInit);
    }

    private void OnPostInit(EntityUid uid, StationRenameHolopadsComponent component, ref StationPostInitEvent args)
    {
        SyncHolopadsNames(uid);
    }

    private void SyncHolopadsNames(EntityUid stationUid)
    {
        // update all holopads that belong to this station grid
        var query = EntityQueryEnumerator<HolopadComponent>();
        while (query.MoveNext(out var uid, out var pad))
        {
            if (!(pad.UseStationName || pad.GetHolopadNameFromShip))
                continue;

            var padStationUid = _stationSystem.GetOwningStation(uid);
            if (padStationUid != stationUid)
                continue;

            SyncHolopad((uid, pad), padStationUid);
        }
    }

    public void SyncHolopad(Entity<HolopadComponent> holopad, EntityUid? padStationUid = null, bool forceit =  false)
    {
        if (!(holopad.Comp.UseStationName || holopad.Comp.GetHolopadNameFromShip) && !forceit)
            return;

        padStationUid ??= _stationSystem.GetOwningStation(holopad);
        string baseName = "";

        if (padStationUid != null)
        {
            baseName = Name(padStationUid.Value);
        }
        else if (TryComp<TransformComponent>(holopad, out var xform) && xform.GridUid is EntityUid grid)
        {
            baseName = MetaData(grid).EntityName;
        }
        else
        {
            return;
        }

        var padName = "";

        if (!string.IsNullOrEmpty(holopad.Comp.StationNamePrefix))
        {
            padName += holopad.Comp.StationNamePrefix + " ";
        }

        padName += baseName;

        if (!string.IsNullOrEmpty(holopad.Comp.StationNameSuffix))
        {
            padName += " " + holopad.Comp.StationNameSuffix;
        }

        _label.Label(holopad, padName);
    }
}
