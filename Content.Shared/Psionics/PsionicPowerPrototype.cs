using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Psionics;

[Prototype("psionicPower")]
public sealed class PsionicPowerPrototype : IPrototype
{
    /// <summary>
    ///     The ID of the psionic power to use.
    /// </summary>
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    ///     The description of a power in yml, used for player notifications.
    /// </summary>
    [DataField(required: true)]
    public string Description = string.Empty;

    /// <summary>
    ///     The list of each Action that this power adds in the form of ActionId and ActionEntity
    /// </summary>
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public readonly Dictionary<string, EntityUid>? Actions = new();

    /// <summary>
    ///     The list of what Components this power adds.
    /// </summary>
    [DataField]
    public readonly List<Component>? Components = new();

    /// <summary>
    ///     What message will play as a popup when the power is initialized.
    /// </summary>
    [DataField(required: true)]
    public string InitializationFeedback = default!;

    /// <summary>
    ///     What message will this power generate when scanned by a Metempsionic Focused Pulse.
    /// </summary>
    [DataField(required: true)]
    public string MetempsionicFeedback = default!;

    /// <summary>
    ///     How much this power will increase or decrease a user's Amplification when initialized.
    /// </summary>
    [DataField]
    public float AmplificationModifier = 0;

    /// <summary>
    ///     How much this power will increase or decrease a user's Dampening when initialized.
    /// </summary>
    [DataField]
    public float DampeningModifier = 0;
}