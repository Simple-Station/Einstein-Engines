using Content.Server.Cargo.Components;
using Content.Server.Cargo.Systems;
using Content.Server.CartridgeLoader.Cartridges;
using Content.Server.Chat.Systems;
using Content.Server.Station.Systems;
using Content.Server.StationRecords.Systems;
using Content.Shared.Chat;
using Content.Shared.Delivery;
using Content.Shared.Destructible;
using Content.Shared.FingerprintReader;
using Content.Shared.Labels.EntitySystems;
using Content.Shared.StationRecords;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Server.Delivery;

/// <summary>
/// System for managing deliveries spawned by the mail teleporter.
/// This covers for mail spawning, as well as granting cargo money.
/// </summary>
public sealed partial class DeliverySystem : SharedDeliverySystem
{
    [Dependency] private readonly CargoSystem _cargo = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly StationRecordsSystem _records = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly FingerprintReaderSystem _fingerprintReader = default!;
    [Dependency] private readonly SharedLabelSystem _label = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly MailMetricsCartridgeSystem _mailMetricsCartridge = default!;
    [Dependency] private readonly LogisticStatsSystem _logisticsStats = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DeliveryComponent, MapInitEvent>(OnMapInit);

        InitializeSpawning();
    }

    private void OnMapInit(Entity<DeliveryComponent> ent, ref MapInitEvent args)
    {
        _container.EnsureContainer<Container>(ent, ent.Comp.Container);

        var stationId = _station.GetStationInMap(Transform(ent).MapID);

        if (stationId == null)
            return;

        _records.TryGetRandomRecord<GeneralStationRecord>(stationId.Value, out var entry);
        _mailMetricsCartridge.UpdateAllCartridges(stationId.Value);

        if (entry == null)
            return;

        ent.Comp.RecipientName = entry.Name;
        ent.Comp.RecipientJobTitle = entry.JobTitle;
        ent.Comp.RecipientStation = stationId.Value;

        _appearance.SetData(ent, DeliveryVisuals.JobIcon, entry.JobIcon);

        _label.Label(ent, ent.Comp.RecipientName);

        if (TryComp<FingerprintReaderComponent>(ent, out var reader) && entry.Fingerprint != null)
        {
            _fingerprintReader.AddAllowedFingerprint((ent.Owner, reader), entry.Fingerprint);
        }

        Dirty(ent);
    }

    protected override void OnDestruction(Entity<DeliveryComponent> ent, ref DestructionEventArgs args)
    {
        base.OnDestruction(ent, ref args);

        if (!ent.Comp.IsLocked)
            return;

        ExecuteForEachLogisticsStats(
            ent,
            (station, logisticStats) =>
            {
                _logisticsStats.AddTamperedMailLosses(
                    station,
                    logisticStats,
                    ent.Comp.IsProfitable ? ent.Comp.SpesoPenalty : 0);
            });

        WithdrawSpesoPenalty(ent.AsNullable());
    }

    protected override void OnBreak(Entity<DeliveryComponent> ent, ref BreakageEventArgs args)
    {
        base.OnBreak(ent, ref args);

        if (!ent.Comp.IsFragile)
            return;

        WithdrawSpesoPenalty(ent.AsNullable());
        ExecuteForEachLogisticsStats(
            ent,
            (station, logisticStats) =>
            {
                _logisticsStats.AddDamagedMailLosses(
                    station,
                    logisticStats,
                    ent.Comp.IsProfitable ? ent.Comp.SpesoPenalty : 0);
            });
    }

    private void ExecuteForEachLogisticsStats(EntityUid uid, Action<EntityUid, StationLogisticStatsComponent> action)
    {
        var query = EntityQueryEnumerator<StationLogisticStatsComponent>();
        while (query.MoveNext(out var station, out var logisticStats))
        {
            if (_station.GetOwningStation(uid) != station)
                continue;

            action(station, logisticStats);
        }
    }

    protected override void OpenDelivery(Entity<DeliveryComponent> ent, EntityUid user, bool attemptPickup = true)
    {
        base.OpenDelivery(ent, user, attemptPickup);

        ExecuteForEachLogisticsStats(
            ent,
            (station, logisticStats) =>
            {
                _logisticsStats.AddOpenedMailEarnings(
                    station,
                    logisticStats,
                    ent.Comp.IsProfitable ? ent.Comp.SpesoReward : 0);
            });
    }

    protected override void GrantSpesoReward(Entity<DeliveryComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        if (!ent.Comp.IsProfitable)
            return;

        if (!TryComp<StationBankAccountComponent>(ent.Comp.RecipientStation, out var account))
            return;

        _cargo.UpdateBankAccount(ent, account, ent.Comp.SpesoReward);
    }

    protected override void WithdrawSpesoPenalty(Entity<DeliveryComponent?> ent)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        if (ent.Comp.IsPriority)
            _appearance.SetData(ent, DeliveryVisuals.IsPriorityInactive, true);

        if (!ent.Comp.IsProfitable)
            return;

        _chat.TrySendInGameICMessage(ent, Loc.GetString("delivery-penalty", ("credits", ent.Comp.SpesoPenalty)), InGameICChatType.Speak, false);
        _audio.PlayPvs(ent.Comp.PenaltySound, ent);

        if (!TryComp<StationBankAccountComponent>(ent.Comp.RecipientStation, out var account))
            return;

        _cargo.UpdateBankAccount(ent, account, ent.Comp.SpesoPenalty);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateSpawner(frameTime);
    }
}
