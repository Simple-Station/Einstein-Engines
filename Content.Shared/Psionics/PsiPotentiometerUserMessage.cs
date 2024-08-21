using Robust.Shared.Serialization;

namespace Content.Shared.Psionics;
[Serializable, NetSerializable]
public sealed class PsiPotentiometerUserMessage : BoundUserInterfaceMessage
{
    public readonly NetEntity? TargetEntity;

    public List<string>? MetapsionicFeedback;

    public PsiPotentiometerUserMessage(NetEntity? targetEntity, List<string>? metapsionicFeedback)
    {
        TargetEntity = targetEntity;
        MetapsionicFeedback = metapsionicFeedback;
    }
}