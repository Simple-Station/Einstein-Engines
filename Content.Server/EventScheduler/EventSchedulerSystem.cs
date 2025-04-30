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

    private uint NextId() { return _id++; }

    private void Enqueue(DelayedEvent delayedEvent, TimeSpan time)
    {
        _eventList.Add(delayedEvent.Id, delayedEvent);
        _eventQueue.Enqueue(delayedEvent.Id, time);
    }

    private void Dequeue(out DelayedEvent? delayedEvent)
    {
        var id = _eventQueue.Dequeue();
        delayedEvent = _eventList[id];
        _eventList.Remove(id);
    }

    private void Dequeue()
    {
        Dequeue(out _);
    }

    public DelayedEvent ScheduleEvent<TEvent>(EntityUid uid, ref TEvent eventArgs, TimeSpan time)
        where TEvent : notnull
    {
        var delayedEvent = new DelayedEvent(NextId(), uid, eventArgs);
        Enqueue(delayedEvent, time);

        Log.Debug($"Scheduled {eventArgs.GetType()} event for {uid}");

        return delayedEvent;
    }

    public DelayedEvent DelayEvent<TEvent>(EntityUid uid, ref TEvent eventArgs, TimeSpan delay)
        where TEvent : notnull
    {
        return ScheduleEvent(uid, ref eventArgs, _gameTiming.CurTime + delay);
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
        const uint failsafe = 1000;
        uint iterationCount = 0;
        while (true)
        {
            // this should never happen
            iterationCount++;
            if (iterationCount >= failsafe)
                break;

            // mostly a getter for values we're dealing with, if there are no queued events break
            if (!_eventQueue.TryPeek(out var index, out var time)
                || !_eventList.TryGetValue(index, out var current))
                break;

            // if the pointed event has been cancelled, get the next event
            if (current.Cancelled)
            {
                Dequeue();

                Log.Debug($"Event cancelled for {current.Uid}!");
                continue;
            }

            // if the pointed event can be triggered, raise it and get the next event
            // this is in case >1 event is raised at the same time, allowing them to trigger on the same frame
            if (_gameTiming.CurTime >= time)
            {
                Dequeue();
                RaiseLocalEvent(current.Uid, current.EventArgs);

                Log.Debug($"Event raised for {current.Uid}!");
                continue;
            }

            // exit loop if nothing happens
            break;
        }
    }
}
