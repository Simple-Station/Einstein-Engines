using Content.Shared.EventScheduler;
using Robust.Shared.Timing;

namespace Content.Server.EventScheduler;

public sealed class EventSchedulerSystem : SharedEventSchedulerSystem
{
    //TODO: move server files to shared after System.Collection.Generic.PriorityQueue`2 is whitelisted in sandbox.yml in RobustToolbox
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private uint _id = 0;
    private uint NextId() { return _id++; }

    private Dictionary<uint, DelayedEvent> _eventDict = new();
    private static PriorityQueue<uint, TimeSpan> _eventQueue = new(_comparer);
    private static EventSchedulerComparer _comparer = new();

    private void Enqueue(DelayedEvent delayedEvent, TimeSpan time)
    {
        _eventDict.Add(delayedEvent.Id, delayedEvent);
        _eventQueue.Enqueue(delayedEvent.Id, time);
    }

    private void Dequeue(out DelayedEvent? delayedEvent)
    {
        var id = _eventQueue.Dequeue();
        delayedEvent = _eventDict[id];
        _eventDict.Remove(id);
    }

    private void Dequeue()
    {
        Dequeue(out _);
    }

    private bool TryRequeue(DelayedEvent delayedEvent, TimeSpan time, bool useDelay = false)
    {
        var curId = delayedEvent.Id;

        // if we can't get the event for whatever reason, consider the requeuing a failure
        if (!_eventDict.TryGetValue(curId, out _))
        {
            Log.Warning($"Couldn't reschedule event for {delayedEvent.Uid}, missing a value!");

            return false;
        }

        // if we cannot remove the event for whatever reason, consider requeuing a failure
        if (!_eventQueue.Remove(curId, out _, out var originalTime))
        {
            Log.Warning($"Couldn't reschedule event for {delayedEvent.Uid}, failed to remove!");

            return false;
        }

        // if we the delay behaviour, consider the original time as the starting point and our input as a delay
        if (useDelay)
            time += originalTime;

        // finally requeue
        _eventQueue.Enqueue(curId, time);
        return true;
    }

    /// <summary>
    /// Wraps an Event to be raised at a specific time in the future.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="uid">The EntityUid which the Event will be raised for.</param>
    /// <param name="eventArgs">The Event to be passed to the scheduler.</param>
    /// <param name="time">The time at which the Event will be raised.</param>
    /// <returns>A DelayedEvent instance. Keep this if you want to conditionally reschedule your Event.</returns>
    public DelayedEvent ScheduleEvent<TEvent>(EntityUid uid, ref TEvent eventArgs, TimeSpan time)
        where TEvent : notnull
    {
        var delayedEvent = new DelayedEvent(NextId(), uid, eventArgs);
        Enqueue(delayedEvent, time);

        Log.Debug($"Scheduled event: '{eventArgs.GetType()}' at uid: ({uid}) for time: ({time})");

        return delayedEvent;
    }

    /// <summary>
    /// Wraps an Event to be raised after a time has elapsed.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="uid">The EntityUid which the Event will be raised for.</param>
    /// <param name="eventArgs">The Event to be passed to the scheduler.</param>
    /// <param name="delay">A delay after which the Event will be raised.</param>
    /// <returns>A DelayedEvent instance. Keep this if you want to conditionally reschedule your Event.</returns>
    public DelayedEvent DelayEvent<TEvent>(EntityUid uid, ref TEvent eventArgs, TimeSpan delay)
        where TEvent : notnull
    {
        return ScheduleEvent(uid, ref eventArgs, _gameTiming.CurTime + delay);
    }

    /// <summary>
    /// Takes an existing DelayedEvent and reschedules it to a specific time, as long as it hasn't already been raised yet.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="delayedEvent">The DelayedEvent instance which you want to reschedule.</param>
    /// <param name="time">The new time at which the Event will be raised.</param>
    /// <returns>Returns true if the DelayedEvent exists, false otherwise.</returns>
    public bool TryRescheduleDelayedEvent(DelayedEvent delayedEvent, TimeSpan time)
    {
        if (!TryRequeue(delayedEvent, time))
            return false;

        Log.Debug($"Rescheduled event: '{delayedEvent.EventArgs.GetType()}' at uid: ({delayedEvent.Uid}) for time: ({time})");
        return true;
    }

    /// <summary>
    /// Takes an existing DelayedEvent and postpones it by a certain time, as long as it hasn't already been raised yet.
    /// </summary>
    /// <typeparam name="TEvent"></typeparam>
    /// <param name="delayedEvent">The DelayedEvent instance which you want to reschedule.</param>
    /// <param name="delay">A delay that is added to the Event's scheduled raise time.</param>
    /// <returns>Returns true if the DelayedEvent exists, false otherwise.</returns>
    public bool TryPostponeDelayedEvent(DelayedEvent delayedEvent, TimeSpan delay)
    {
        if (!TryRequeue(delayedEvent, delay, true))
            return false;

        Log.Debug($"Postponed event: '{delayedEvent.EventArgs.GetType()}' at uid: ({delayedEvent.Uid}) by delay: ({delay})");
        return true;
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
            {
                Log.Warning($"Event processing hit safety limit of {failsafe} events in one frame - possible infinite loop detected!");
                break;
            }

            // mostly a getter for values we're dealing with, if there are no queued events break
            if (!_eventQueue.TryPeek(out var index, out var time)
                || !_eventDict.TryGetValue(index, out var current))
                break;

            // if the pointed event has been cancelled, get the next event
            if (current.Cancelled)
            {
                Dequeue();

                Log.Debug($"Cancelled event '{current.EventArgs.GetType()}' at uid: ({current.Uid})!");
                continue;
            }

            // if the pointed event can be triggered, raise it and get the next event
            // this is in case >1 event is raised at the same time, allowing them to trigger on the same frame
            if (_gameTiming.CurTime >= time)
            {
                Dequeue();

                try { RaiseLocalEvent(current.Uid, current.EventArgs); }
                catch (Exception ex) { Log.Error($"Error processing event for entity {current.Uid}: {ex}"); }

                Log.Debug($"Raised event '{current.EventArgs.GetType()}' at uid: ({current.Uid})!");
                continue;
            }

            // exit loop if nothing happens
            break;
        }
    }
}
