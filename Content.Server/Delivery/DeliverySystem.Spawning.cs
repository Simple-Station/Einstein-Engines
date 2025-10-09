using System.Linq;
using System.Threading;
using Content.Server.Damage.Components;
using Content.Server.Destructible;
using Content.Server.Destructible.Thresholds;
using Content.Server.Destructible.Thresholds.Behaviors;
using Content.Server.Destructible.Thresholds.Triggers;
using Content.Server.Power.EntitySystems;
using Content.Server.StationRecords;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Damage;
using Content.Shared.Delivery;
using Content.Shared.EntityTable;
using Content.Shared.Fluids.Components;
using Content.Shared.Nutrition.Components;
using Content.Shared.Nutrition.EntitySystems;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Timer = Robust.Shared.Timing.Timer;


namespace Content.Server.Delivery;

/// <summary>
/// System for managing deliveries spawned by the mail teleporter.
/// This covers for spawning deliveries.
/// </summary>
public sealed partial class DeliverySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EntityTableSystem _entityTable = default!;
    [Dependency] private readonly PowerReceiverSystem _power = default!;
    [Dependency] private readonly OpenableSystem _openable = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;

    private void InitializeSpawning()
    {
        SubscribeLocalEvent<CargoDeliveryDataComponent, MapInitEvent>(OnDataMapInit);
    }

    private void OnDataMapInit(Entity<CargoDeliveryDataComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.NextDelivery = _timing.CurTime + ent.Comp.MinDeliveryCooldown; // We want an early wave of mail so cargo doesn't have to wait
    }

    private void SpawnDelivery(Entity<DeliverySpawnerComponent?> ent, CargoDeliveryDataComponent deliveryData, int amount)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        var coords = Transform(ent).Coordinates;

        _audio.PlayPvs(ent.Comp.SpawnSound, ent.Owner);

        for (int i = 0; i < amount; i++)
        {
            var spawns = _entityTable.GetSpawns(ent.Comp.Table);

            foreach (var id in spawns)
            {
                SetupDelivery(Spawn(id, coords), deliveryData);
            }
        }
    }

    private void SpawnStationDeliveries(Entity<CargoDeliveryDataComponent> ent)
    {
        if (!TryComp<StationRecordsComponent>(ent, out var records))
            return;

        var spawners = GetValidSpawners(ent);

        // Skip if theres no spawners available
        if (spawners.Count == 0)
            return;

        // We take the amount of mail calculated based on player amount or the minimum, whichever is higher.
        // We don't want stations with less than the player ratio to not get mail at all
        var deliveryCount = Math.Max(records.Records.Keys.Count / ent.Comp.PlayerToDeliveryRatio, ent.Comp.MinimumDeliverySpawn);

        if (!ent.Comp.DistributeRandomly)
        {
            foreach (var spawner in spawners)
            {
                SpawnDelivery(spawner, ent.Comp, deliveryCount);
            }
        }
        else
        {
            int[] amounts = new int[spawners.Count];

            // Distribute items randomly
            for (int i = 0; i < deliveryCount; i++)
            {
                var randomListIndex = _random.Next(spawners.Count);
                amounts[randomListIndex]++;
            }
            for (int j = 0; j < spawners.Count; j++)
            {
                SpawnDelivery(spawners[j], ent.Comp, amounts[j]);
            }
        }

    }

    private List<EntityUid> GetValidSpawners(Entity<CargoDeliveryDataComponent> ent)
    {
        var validSpawners = new List<EntityUid>();

        var spawners = EntityQueryEnumerator<DeliverySpawnerComponent>();
        while (spawners.MoveNext(out var spawnerUid, out _))
        {
            var spawnerStation = _station.GetOwningStation(spawnerUid);

            if (spawnerStation != ent.Owner)
                continue;

            if (!_power.IsPowered(spawnerUid))
                continue;

            validSpawners.Add(spawnerUid);
        }

        return validSpawners;
    }

    private void UpdateSpawner(float frameTime)
    {
        var dataQuery = EntityQueryEnumerator<CargoDeliveryDataComponent>();
        var curTime = _timing.CurTime;

        while (dataQuery.MoveNext(out var uid, out var deliveryData))
        {
            if (deliveryData.NextDelivery > curTime)
                continue;

            deliveryData.NextDelivery += _random.Next(deliveryData.MinDeliveryCooldown, deliveryData.MaxDeliveryCooldown); // Random cooldown between min and max
            SpawnStationDeliveries((uid, deliveryData));
        }
    }

    private void SetupDelivery(Entity<DeliveryComponent?> ent, CargoDeliveryDataComponent cargoDeliveryData)
    {
        if (!Resolve(ent, ref ent.Comp))
            return;

        if (!_container.TryGetContainer(ent, ent.Comp.Container, out var container))
            return;

        if (ent.Comp.IsPriority || _random.Prob(cargoDeliveryData.PriorityChance))
        {
            ent.Comp.IsPriority = true;

            ent.Comp.SpesoReward += cargoDeliveryData.PriorityBonus;
            ent.Comp.SpesoPenalty += cargoDeliveryData.PriorityMalus;
            _appearance.SetData(ent, DeliveryVisuals.IsPriority, true);

            ent.Comp.PriorityCancelToken = new CancellationTokenSource();

            Timer.Spawn(
                (int) cargoDeliveryData.PriorityDuration.TotalMilliseconds,
                () =>
                {
                    ExecuteForEachLogisticsStats(
                        ent,
                        (station, logisticStats) =>
                    {
                        _logisticsStats.AddExpiredMailLosses(
                            station,
                            logisticStats,
                            ent.Comp.IsProfitable ? ent.Comp.SpesoPenalty : 0);
                    });

                    WithdrawSpesoPenalty(ent.AsNullable());
                },
                ent.Comp.PriorityCancelToken.Token);
        }

        if (!ent.Comp.IsFragile)
        {
            foreach (var entity in container.ContainedEntities.ToArray())
            {
                if (!IsFragile(entity, cargoDeliveryData.FragileDamageThreshold))
                    continue;

                ent.Comp.IsFragile =  true;
                break;
            }

            if (!ent.Comp.IsFragile)
                return;
        }

        ent.Comp.SpesoReward += cargoDeliveryData.FragileBonus;
        ent.Comp.SpesoPenalty += cargoDeliveryData.FragileMalus;
        _appearance.SetData(ent, DeliveryVisuals.IsFragile, true);
    }

    /// <summary>
    /// Returns true if the given entity is considered fragile for delivery.
    /// </summary>
    private bool IsFragile(EntityUid uid, int fragileDamageThreshold)
    {
        // It takes damage on falling.
        if (HasComp<DamageOnLandComponent>(uid))
            return true;

        // It can be spilled easily and has something to spill.
        if (HasComp<SpillableComponent>(uid)
            && TryComp<OpenableComponent>(uid, out var openable)
            && !_openable.IsClosed(uid, null, openable)
            && _solution.PercentFull(uid) > 0)
            return true;

        // It might be made of non-reinforced glass.
        if (TryComp<DamageableComponent>(uid, out var damageableComponent)
            && damageableComponent.DamageModifierSetId == "Glass")
            return true;

        // Fallback: It breaks or is destroyed in less than a damage
        // threshold dictated by the teleporter.
        if (!TryComp<DestructibleComponent>(uid, out var destructibleComp))
            return false;

        foreach (var threshold in destructibleComp.Thresholds)
        {
            if (threshold.Trigger is not DamageTrigger trigger || trigger.Damage >= fragileDamageThreshold)
                continue;

            foreach (var behavior in threshold.Behaviors)
            {
                if (behavior is not DoActsBehavior doActs)
                    continue;

                if (doActs.Acts.HasFlag(ThresholdActs.Breakage) || doActs.Acts.HasFlag(ThresholdActs.Destruction))
                    return true;
            }
        }

        return false;
    }
}
