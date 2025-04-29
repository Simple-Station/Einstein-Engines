using Robust.Shared.Timing;


namespace Content.Shared.EventScheduler;

public abstract partial class SharedEventSchedulerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private static uint _index = 0;
    private static Dictionary<uint, DelayedEvent> _eventList = new();
    private static PriorityQueue<uint, TimeSpan> _eventQueue = new(_comparer);
    private static EventSchedulerComparer _comparer = new();

    public DelayedEvent ScheduleEvent(EntityUid uid, object eventArgs, TimeSpan time)
    {
        var delayedEvent = new DelayedEvent(uid, eventArgs);

        _eventList.Add(_index, delayedEvent);
        _eventQueue.Enqueue(_index, time);

        _index++;

        return delayedEvent;
    }

    public DelayedEvent DelayEvent(EntityUid uid, object eventArgs, TimeSpan delay)
    {
        return ScheduleEvent(uid, eventArgs, _gameTiming.CurTime + delay);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
    }
}

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

public record struct DelayedEvent(EntityUid Uid, object EventArgs, bool Cancelled = false) { }
