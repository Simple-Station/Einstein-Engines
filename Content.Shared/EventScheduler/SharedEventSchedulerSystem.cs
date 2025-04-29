using Robust.Shared.Timing;


namespace Content.Shared.EventScheduler;

public abstract partial class SharedEventSchedulerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private static uint _index = 0;
    private static Dictionary<uint, object> _eventList = new();
    private static PriorityQueue<object, TimeSpan> _eventQueue = new(_comparer);
    private static EventSchedulerComparer _comparer = new();

    public void Enqueue(object delayedEvent, TimeSpan delay)
    {
        _eventList.Add(_index, delayedEvent);
        _eventQueue.Enqueue(_index, _gameTiming.CurTime + delay);

        _index++;
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
