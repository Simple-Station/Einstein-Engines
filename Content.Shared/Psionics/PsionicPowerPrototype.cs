using Content.Shared.Chat;
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
    ///     What message will be sent to the player as a Popup.
    ///     If left blank, it will default to the Const "generic-power-initialization-feedback"
    /// </summary>
    [DataField]
    public string? InitializationPopup;

    /// <summary>
    ///     What message will be sent to the chat window when the power is initialized. Leave it blank to send no message.
    ///     Initialization messages won't play for powers that are Innate, only powers obtained during the round.
    ///     These should generally also be written in the first person, and can be far lengthier than popups.
    /// </summary>
    [DataField]
    public string? InitializationFeedback;

    /// <summary>
    ///     What color will the initialization feedback display in the chat window with.
    /// </summary>
    [DataField]
    public string InitializationFeedbackColor = "#8A00C2";

    /// <summary>
    ///     What font size will the initialization message use in chat.
    /// </summary>
    [DataField]
    public int InitializationFeedbackFontSize = 12;

    /// <summary>
    ///     Which chat channel will the initialization message use.
    /// </summary>
    [DataField]
    public ChatChannel InitializationFeedbackChannel = ChatChannel.Emotes;

    /// <summary>
    ///     What message will this power generate when scanned by a Metempsionic Focused Pulse.
    /// </summary>
    [DataField]
    public string MetapsionicFeedback = "psionic-metapsionic-feedback-default";

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

    /// <summary>
    ///     How many "Power Slots" this power occupies.
    /// </summary>
    [DataField]
    public int PowerSlotCost = 1;
}