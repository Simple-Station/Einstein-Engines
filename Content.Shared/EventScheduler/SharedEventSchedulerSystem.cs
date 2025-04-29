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

        /*
            huh?? weren't you supposed to make an optimised system to replace frameTime?
            -   no, this replaces EQE, uses frametime to stay in sync with game tick
                but it should only iterate 1/frame if no events occur on the frame
        */
        while (true)
        {
            // mostly a getter for values we're dealing with, if we can't get them for whatever reason something is wrong
            if (!_eventQueue.TryPeek(out var index, out var time)
                || !_eventList.TryGetValue(index, out var current))
                break;

            // if the pointed event has been cancelled, get the next event
            if (current.Cancelled)
            {
                _eventQueue.Dequeue();
                continue;
            }

            // if the pointed event can be triggered, raise it and get the next event
            // this is in case >1 event is raised at the same time, allowing them to trigger on the same frame
            if (_gameTiming.CurTime >= time)
            {
                _eventQueue.Dequeue();
                RaiseLocalEvent(current.Uid, current.EventArgs);
                continue;
            }

            // exit loop if nothing happens
            break;
        }
    }
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

public record struct DelayedEvent(EntityUid Uid, object EventArgs, bool Cancelled = false) { }
