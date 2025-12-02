using Robust.Shared.Serialization;

namespace Content.Shared.Shuttles.Events;

/// <summary>
/// Raised on a client IFF console when there is changing color.
/// </summary>
[Serializable, NetSerializable]
public sealed class IFFSetColorMessage : BoundUserInterfaceMessage
{
    public Color Color;

    public IFFSetColorMessage(Color color)
    {
        Color = color;
    }
}
