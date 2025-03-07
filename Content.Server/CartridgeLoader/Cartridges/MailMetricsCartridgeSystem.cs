using Content.Server.Cargo.Components;
using Content.Server.Cargo.Systems;
using Content.Server.Station.Systems;
using Content.Shared.CartridgeLoader;
using Content.Shared.CartridgeLoader.Cartridges;
using Content.Shared.Delivery;

namespace Content.Server.CartridgeLoader.Cartridges;

public sealed class MailMetricsCartridgeSystem : EntitySystem
{
    [Dependency] private readonly CartridgeLoaderSystem _cartridgeLoader = default!;
    [Dependency] private readonly StationSystem _station = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MailMetricsCartridgeComponent, CartridgeUiReadyEvent>(OnUiReady);
        SubscribeLocalEvent<LogisticStatsUpdatedEvent>(OnLogisticsStatsUpdated);
    }

    private void OnUiReady(Entity<MailMetricsCartridgeComponent> ent, ref CartridgeUiReadyEvent args)
    {
        UpdateUI(ent, args.Loader);
    }

    private void OnLogisticsStatsUpdated(LogisticStatsUpdatedEvent args)
    {
        UpdateAllCartridges(args.Station);
    }

    public void UpdateAllCartridges(EntityUid station)
    {
        var query = EntityQueryEnumerator<MailMetricsCartridgeComponent, CartridgeComponent>();
        while (query.MoveNext(out var uid, out var comp, out var cartridge))
        {
            if (cartridge.LoaderUid is not { } loader || comp.Station != station)
                continue;
            UpdateUI((uid, comp), loader);
        }
    }

    private void UpdateUI(Entity<MailMetricsCartridgeComponent> ent, EntityUid loader)
    {
        if (_station.GetOwningStation(loader) is { } station)
            ent.Comp.Station = station;

        if (!TryComp<StationLogisticStatsComponent>(ent.Comp.Station, out var logiStats))
            return;

        // Get station's logistic stats
        var unopenedMailCount = GetUnopenedMailCount(ent.Comp.Station);

        // Send logistic stats to cartridge client
        var state = new MailMetricUiState(logiStats.Metrics, unopenedMailCount);
        _cartridgeLoader.UpdateCartridgeUiState(loader, state);
    }


    private int GetUnopenedMailCount(EntityUid? station)
    {
        var unopenedMail = 0;

        var query = EntityQueryEnumerator<DeliveryComponent>();

        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.IsLocked && _station.GetOwningStation(uid) == station)
                unopenedMail++;
        }

        return unopenedMail;
    }
}
