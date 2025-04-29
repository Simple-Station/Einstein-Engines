using Content.Shared.EventScheduler;
using Robust.Shared.Timing;

namespace Content.Server.EventScheduler;

public sealed class EventSchedulerSystem : SharedEventSchedulerSystem
{
    //TODO: move server files to shared after System.Collection.Generic.PriorityQueue`2 is whitelisted in sandbox.yml in RobustToolbox
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private static uint _id = 0;
    private static Dictionary<uint, DelayedEvent> _eventList = new();
    private static PriorityQueue<uint, TimeSpan> _eventQueue = new(_comparer);
    private static EventSchedulerComparer _comparer = new();

    private uint NextId()
    {
        return _id++ - 1;
    }

    public DelayedEvent ScheduleEvent(EntityUid uid, object eventArgs, TimeSpan time)
    {
        var delayedEvent = new DelayedEvent(uid, eventArgs);
        var id = NextId();

        _eventList.Add(id, delayedEvent);
        _eventQueue.Enqueue(id, time);

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
                REGARDLESS OF HOW MANY SYSTEMS USE IT <-------
        */
        while (true)
        {
            // mostly a getter for values we're dealing with, if there are no queued events break
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
