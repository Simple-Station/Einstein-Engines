using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Humanoid;
using Robust.Shared.Prototypes;

namespace Content.Server.WhiteDream.BloodCult.Runes;

[RegisterComponent]
public sealed partial class CultRuneBaseComponent : Component
{
    [DataField(required: true)]
    public string InvokePhrase = "";

    [DataField]
    public InGameICChatType InvokeChatType = InGameICChatType.Whisper;

    [DataField]
    public int RequiredInvokers = 1;

    [DataField]
    public float RuneActivationRange = 1f;

    /// <summary>
    ///     Damage dealt on the rune activation.
    /// </summary>
    [DataField]
    public DamageSpecifier? ActivationDamage;

    public EntProtoId HolyWaterPrototype = "HolyWater";
}

public sealed class TryInvokeCultRuneEvent(EntityUid user, HashSet<Entity<HumanoidAppearanceComponent>> invokers)
    : CancellableEntityEventArgs
{
    public EntityUid User = user;
    public HashSet<Entity<HumanoidAppearanceComponent>> Invokers = invokers;
}

public sealed class AfterRunePlaced(EntityUid user)
{
    public EntityUid User = user;
}
