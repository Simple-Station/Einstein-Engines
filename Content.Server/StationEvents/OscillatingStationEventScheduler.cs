using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Shared.GameTicking.Components;
using Content.Server.StationEvents.Components;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.StationEvents;

public sealed class OscillatingStationEventSchedulerSystem : GameRuleSystem<OscillatingStationEventSchedulerComponent>
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly EventManagerSystem _event = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    [Conditional("DEBUG")]
    private void DebugValidateParams(OscillatingStationEventSchedulerComponent c)
    {
        // This monstrousity is necessary because if someone fucks up one of these parameters,
        // it will likely either crash the game (in debug), or cause the event scheduler to stop working and spam the server console (in prod)
        DebugTools.Assert(c.DownwardsBias <= 0f && c.UpwardsBias >= 0f, "Fix your scheduler bias");
        DebugTools.Assert(c.DownwardsLimit <= 0f && c.UpwardsLimit >= 0f, "Fix your scheduler slope limits");
        DebugTools.Assert(c.UpdateInterval > TimeSpan.Zero, "Scheduler update interval must be positive");
        DebugTools.Assert(c.ChaosStickiness >= 0f && c.ChaosStickiness <= 1f, "Scheduler stickiness must be between 0 and 1");
        DebugTools.Assert(c.SlopeStickiness >= 0f && c.SlopeStickiness <= 1f, "Scheduler stickiness must be between 0 and 1");
        DebugTools.Assert(c.MinChaos < c.MaxChaos, "Don't set the minimum above the maximum");
    }

    private TimeSpan CalculateAverageEventTime(OscillatingStationEventSchedulerComponent comp)
    {
        //TODO Those cvars are bad
        var min = _cfg.GetCVar(CCVars.GameEventsOscillatingMinimumTime);
        var max = _cfg.GetCVar(CCVars.GameEventsOscillatingAverageTime);

        return TimeSpan.FromSeconds(min + (max - min) / comp.CurrentChaos); // Why does C# have no math.lerp??????????????
    }

    protected override void Started(EntityUid uid, OscillatingStationEventSchedulerComponent comp, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        DebugValidateParams(comp);

        comp.CurrentChaos = comp.MinChaos + comp.StartingChaosRatio * (comp.MaxChaos - comp.MinChaos);
        comp.CurrentSlope = comp.StartingSlope;

        comp.NextUpdate = _timing.CurTime + CalculateAverageEventTime(comp);
        comp.LastEventTime = _timing.CurTime; // Just so we don't run an event the very moment this scheduler gets added
    }

    protected override void ActiveTick(EntityUid uid, OscillatingStationEventSchedulerComponent comp, GameRuleComponent gameRule, float frameTime)
    {
        if (comp.NextUpdate > _timing.CurTime)
            return;
        comp.NextUpdate = _timing.CurTime + comp.UpdateInterval;
        DebugValidateParams(comp);

        // Slope is the first derivative of chaos, and acceleration is the second
        // We randomize acceleration on each tick and simulate its effect on the slope and base function
        // But we spread the effect across a longer time span to achieve a smooth and pleasant result
        var delta = (float) comp.UpdateInterval.TotalSeconds;
        var newAcceleration = _random.NextFloat(comp.DownwardsBias, comp.UpwardsBias);
        var newSlope =
            Math.Clamp(comp.CurrentSlope + newAcceleration * delta, comp.DownwardsLimit, comp.UpwardsLimit) * (1 - comp.SlopeStickiness)
            + comp.CurrentSlope * comp.SlopeStickiness;
        var newChaos =
            Math.Clamp(comp.CurrentChaos + newSlope * delta, comp.MinChaos, comp.MaxChaos) * (1 - comp.ChaosStickiness)
            + comp.CurrentChaos * comp.ChaosStickiness;

        comp.CurrentChaos = newChaos;
        comp.CurrentSlope = newSlope;
        comp.LastAcceleration = newAcceleration;

        // We do not use fixed "next event" times because that can cause us to skip over chaos spikes due to periods of low chaos
        // Instead we recalculate the time until next event every time, so it can change before the event is even started
        var targetDelay = CalculateAverageEventTime(comp);
        if (_timing.CurTime > comp.LastEventTime + targetDelay && TryRunNextEvent(uid, comp, out _))
        {
            #if DEBUG
                var passed = _timing.CurTime - comp.LastEventTime;
                Log.Debug($"Running an event after {passed.TotalSeconds} sec since last event. Next event scheduled in {CalculateAverageEventTime(comp).TotalSeconds} sec.");
            #endif

            comp.LastEventTime = _timing.CurTime;
        }
    }

    public bool TryRunNextEvent(EntityUid uid, OscillatingStationEventSchedulerComponent comp, [NotNullWhen(true)] out string? runEvent)
    {
        runEvent = _event.PickRandomEvent();
        if (runEvent == null)
            return false;

        _gameTicker.AddGameRule(runEvent);
        return true;
    }
}
