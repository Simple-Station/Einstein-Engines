using Robust.Shared.Timing;


namespace Content.Shared.EventScheduler;

public abstract partial class SharedEventSchedulerSystem : EntitySystem
{
    //TODO: move server files to shared after System.Collection.Generic.PriorityQueue`2 is whitelisted in sandbox.yml in RobustToolbox
}

// comparator function for priority queue, prefers the element that will occur earlier
public sealed class EventSchedulerComparer : IComparer<TimeSpan>
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    public int Compare(TimeSpan a, TimeSpan b)
    {
        TimeSpan aDelay = a - _gameTiming.CurTime;
        TimeSpan bDelay = b - _gameTiming.CurTime;

        if (aDelay < bDelay)
            return -1;
        else if (bDelay < aDelay)
            return 1;

        return 0;
    }
}

public record struct DelayedEvent(EntityUid Uid, object EventArgs, bool Cancelled = false);
