using Content.Shared.Chat;
using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.Runes;

[RegisterComponent]
public sealed partial class CultRuneBaseComponent : Component
{
    public EntProtoId HolyWaterPrototype = "HolyWater";

    [DataField(required: true)]
    public string InvokePhrase = "";

    [DataField]
    public InGameICChatType InvokeChatType = InGameICChatType.Whisper;

    [DataField]
    public int RequiredInvokers = 1;

    [DataField]
    public float InvokersGatherRange = 0.5f; // Roughly half a tile.

    [DataField]
    public float BloodCost = 20;
}

public sealed class TryInvokeCultRuneEvent(EntityUid user, HashSet<EntityUid> invokers)
    : CancellableEntityEventArgs
{
    public EntityUid User = user;
    public HashSet<EntityUid> Invokers = invokers;
}
