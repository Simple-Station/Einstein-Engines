using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.StationEvents.Components;
using Content.Shared.GameTicking.Components;
using Robust.Shared.Random;

namespace Content.Server.StationEvents;

public sealed class RampingStationEventSchedulerSystem : GameRuleSystem<RampingStationEventSchedulerComponent>
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EventManagerSystem _event = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    /// <summary>
    /// Returns the ChaosModifier which increases as round time increases to a point.
    /// </summary>
    public float GetChaosModifier(EntityUid uid, RampingStationEventSchedulerComponent component)
    {
        var roundTime = (float) _gameTicker.RoundDuration().TotalSeconds;
        if (roundTime > component.EndTime)
            return component.MaxChaos;

        return component.MaxChaos / component.EndTime * roundTime + component.StartingChaos;
    }

    protected override void Started(EntityUid uid, RampingStationEventSchedulerComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        // Worlds shittiest probability distribution
        // Got a complaint? Send them to
        component.MaxChaos = component.AverageChaos * _random.NextFloat(0.75f, 1.25f);
        // This is in minutes, so *60 for seconds (for the chaos calc)
        component.EndTime = component.AverageEndTime * _random.NextFloat(0.75f, 1.25f) * 60f;
        component.StartingChaos = component.MaxChaos * component.StartingChaosRatio;

        PickNextEventTime(uid, component);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_event.EventsEnabled)
            return;

        var query = EntityQueryEnumerator<RampingStationEventSchedulerComponent, GameRuleComponent>();
        while (query.MoveNext(out var uid, out var scheduler, out var gameRule))
        {
            if (!GameTicker.IsGameRuleActive(uid, gameRule))
                continue;

            if (scheduler.TimeUntilNextEvent > 0f)
            {
                scheduler.TimeUntilNextEvent -= frameTime;
                continue;
            }

            PickNextEventTime(uid, scheduler);
            _event.RunRandomEvent(scheduler.ScheduledGameRules);
        }
    }

    /// <summary>
    /// Sets the timing of the next event addition.
    /// </summary>
    private void PickNextEventTime(EntityUid uid, RampingStationEventSchedulerComponent component)
    {
        component.TimeUntilNextEvent =
            _random.NextFloat(
                component.MinimumTimeUntilNextEvent,
                component.MaximumTimeUntilNextEvent)
            * component.EventDelayModifier
            / GetChaosModifier(uid, component);
    }
}
