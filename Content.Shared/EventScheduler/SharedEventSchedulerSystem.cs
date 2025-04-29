namespace Content.Shared.EventScheduler;

public abstract partial class SharedEventSchedulerSystem
{

    public PriorityQueue<object, TimeSpan> EventQueue = new();

    public void Enqueue()
    {

    }

    public void Dequeue()
    {

    }
}
