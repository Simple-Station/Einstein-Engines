using System.Linq;
using Content.Server.Lathe;
using Content.Server.Station.Components;
using Content.Shared.DeviceLinking;
using Content.Shared.Lathe;
using Content.Shared.Materials;
using Robust.Shared.Timing;

namespace Content.Server.Materials;

public sealed class MaterialSiloSystem : SharedMaterialSiloSystem
{
    [Dependency] private readonly LatheSystem _lathe = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BecomesStationComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<MaterialSiloComponent, MaterialAmountChangedEvent>(OnMaterialAmountChanged);
    }

    private void OnMaterialAmountChanged(Entity<MaterialSiloComponent> ent, ref MaterialAmountChangedEvent args)
    {
        // Spawning a timer because SetUiState in UpdateUserInterfaceState is being networked before
        // silo's MaterialStorageComponent state gets handled.
        // That causes lathe ui recipe list to not update properly.
        Timer.Spawn(20,
            () =>
            {
                if (!TryComp(ent, out DeviceLinkSourceComponent? source))
                    return;

                foreach (var utilizerSet in source.Outputs.Where(x => x.Key == SourcePort).Select(x => x.Value))
                {
                    foreach (var utilizer in utilizerSet)
                    {
                        if (TryComp(utilizer, out LatheComponent? lathe))
                            _lathe.UpdateUserInterfaceState(utilizer, lathe);
                    }
                }
            });
    }

    private void OnMapInit(Entity<BecomesStationComponent> ent, ref MapInitEvent args)
    {
        Entity<DeviceLinkSourceComponent>? silo = null;
        var siloQuery = AllEntityQuery<MaterialSiloComponent, MaterialStorageComponent, TransformComponent, DeviceLinkSourceComponent>();
        while (siloQuery.MoveNext(out var siloEnt, out _, out _, out var siloXform, out var source))
        {
            if (siloXform.GridUid != ent)
                continue;

            silo = (siloEnt, source);
            break;
        }

        if (silo == null)
            return;

        var utilizerQuery = AllEntityQuery<MaterialSiloUtilizerComponent, MaterialStorageComponent, TransformComponent, DeviceLinkSinkComponent>();
        while (utilizerQuery.MoveNext(out var utilizer, out _, out var storage, out var utilizerXform, out var sink))
        {
            if (utilizerXform.GridUid != ent)
                continue;

            DeviceLink.LinkDefaults(null, silo.Value, utilizer, silo.Value.Comp, sink);
        }
    }
}
