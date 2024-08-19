using Robust.Shared.Prototypes;

namespace Content.Shared.Psionics;

[Prototype]
public sealed partial class PsionicPowerPrototype : IPrototype
{
    /// <summary>
    ///     The ID of the psionic power to use.
    /// </summary>
    [IdDataField]
    public string ID { get; } = default!;

    /// <summary>
    ///     The name of the psionic power.
    /// </summary>
    [DataField(required: true)]
    public string Name = default!;

    /// <summary>
    ///     The description of a power in yml, used for player notifications.
    /// </summary>
    [DataField(required: true)]
    public string Description = default!;

    /// <summary>
    ///     The list of each Action that this power adds in the form of ActionId and ActionEntity
    /// </summary>
    [DataField]
    public List<EntProtoId> Actions = new();

    /// <summary>
    ///     The list of what Components this power adds.
    /// </summary>
    [DataField]
    public ComponentRegistry Components = new();

    /// <summary>
    ///     What message will play as a popup when the power is initialized.
    /// </summary>
    [DataField]
    public string? InitializationFeedback;

    /// <summary>
    ///     What message will this power generate when scanned by a Metempsionic Focused Pulse.
    /// </summary>
    [DataField]
    public string? MetapsionicFeedback;

    /// <summary>
    ///     How much this power will increase or decrease a user's Amplification.
    /// </summary>
    [DataField]
    public float AmplificationModifier = 0;

    /// <summary>
    ///     How much this power will increase or decrease a user's Dampening.
    /// </summary>
    [DataField]
    public float DampeningModifier = 0;
}