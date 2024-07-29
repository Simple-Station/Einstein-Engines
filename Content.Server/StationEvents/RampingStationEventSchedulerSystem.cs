using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.StationEvents.Components;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server.StationEvents;

public sealed class RampingStationEventSchedulerSystem : GameRuleSystem<RampingStationEventSchedulerComponent>
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EventManagerSystem _event = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    /// <summary>
    ///     A <see href="https://www.desmos.com/calculator/87huunvoxq">logistic curve equation</see> used to smooth out the transition between event times at shift start, vs. shift end.
    ///     Depending on the settings used, the end time might not necessarily be the point at which timers hit the floor.
    ///     It is after all, an asymptote.
    /// </summary>
    /// <param name="component"></param>
    /// <param name="startTime"></param>
    /// <param name="endTimeOffset"></param>
    /// <returns></returns>
    public float RampingEventTimeEquation(RampingStationEventSchedulerComponent component, float startTime, float endTimeOffset = 0)
    {
        var endTime = Math.Clamp(endTimeOffset, 0.1f, startTime - 1);
        var shiftLength = Math.Max(1, _cfg.GetCVar(CCVars.EventsRampingAverageEndTime) - component.ShiftLengthOffset);
        return 2 * endTime
            / (1
            	+ MathF.Exp(_cfg.GetCVar(CCVars.EventsRampingAverageChaos)
	            * component.ShiftChaosModifier
	            / shiftLength
	            * endTime
	            * (float) _gameTicker.RoundDuration().TotalSeconds
	            / 60))
                	+ (startTime - endTime);
    }

    protected override void Started(EntityUid uid, RampingStationEventSchedulerComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        PickNextEventTime(component);
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
                return;

            if (scheduler.TimeUntilNextEvent > 0f)
            {
                scheduler.TimeUntilNextEvent -= frameTime;
                return;
            }

            PickNextEventTime(scheduler);
            _event.RunRandomEvent();
        }
    }

    private void PickNextEventTime(RampingStationEventSchedulerComponent component)
    {
        // In case of server hosts being silly and setting maximum time to be lower than minimum time, sanity check the scheduler inputs and sort them by Min/Max
        var minimumTime = MathF.Min(_cfg.GetCVar(CCVars.GameEventsRampingMinimumTime)
	        - _cfg.GetCVar(CCVars.GameEventsRampingMinimumTimeOffset)
	        - component.MinimumEventTimeOffset, _cfg.GetCVar(CCVars.GameEventsRampingMaximumTime)
	        - _cfg.GetCVar(CCVars.GameEventsRampingMaximumTimeOffset)
	        - component.MaximumEventTimeOffset);

		var maximumTime = MathF.Max(_cfg.GetCVar(CCVars.GameEventsRampingMinimumTime)
	        - _cfg.GetCVar(CCVars.GameEventsRampingMinimumTimeOffset)
	        - component.MinimumEventTimeOffset, _cfg.GetCVar(CCVars.GameEventsRampingMaximumTime)
	        - _cfg.GetCVar(CCVars.GameEventsRampingMaximumTimeOffset)
	        - component.MaximumEventTimeOffset);

        // Just in case someone messed up their math, set it to between 6 and 12 seconds. This absolutely isn't ideal
        component.TimeUntilNextEvent = _random.NextFloat(
            RampingEventTimeEquation(component, MathF.Max(0.1f, minimumTime)),
            RampingEventTimeEquation(component, MathF.Max(0.2f, maximumTime)));

        component.TimeUntilNextEvent *= component.EventDelayModifier;
    }
}
