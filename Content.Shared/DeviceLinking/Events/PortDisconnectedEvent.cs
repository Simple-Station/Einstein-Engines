namespace Content.Shared.DeviceLinking.Events
{
    public sealed class PortDisconnectedEvent : EntityEventArgs
    {
        public readonly string Port;
        public readonly EntityUid RemovedPortUid;

        public PortDisconnectedEvent(string port, EntityUid removedPortUid)
        {
            Port = port;
            RemovedPortUid = removedPortUid;
        }
    }
}
