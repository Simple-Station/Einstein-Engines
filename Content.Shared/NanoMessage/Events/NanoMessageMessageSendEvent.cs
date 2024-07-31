namespace Content.Shared.NanoMessage.Events;

public sealed class NanoMessageMessageSendEvent : BoundUserInterfaceMessage
{
    public string Message = default!;
    public ulong RecipientId;
}
