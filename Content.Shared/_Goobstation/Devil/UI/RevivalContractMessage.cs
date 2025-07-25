using Robust.Shared.Player;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Devil.UI;

[Serializable, NetSerializable]
public sealed class RevivalContractMessage(bool accepted) : BoundUserInterfaceMessage
{
    public bool Accepted { get; } = accepted;
}
