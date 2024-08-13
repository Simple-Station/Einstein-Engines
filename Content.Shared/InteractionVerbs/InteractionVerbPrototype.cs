using Content.Shared.Cargo.Prototypes;
using Content.Shared.Chat;
using Content.Shared.DoAfter;
using Content.Shared.InteractionVerbs.Events;
using Content.Shared.Popups;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;
using Robust.Shared.Utility;

#pragma warning disable CS0618 // Type or member is obsolete

namespace Content.Shared.InteractionVerbs;

/// <summary>
///     Represents an action that can be performed on an entity.
/// </summary>
[Prototype("Interaction"), Serializable]
public sealed partial class InteractionVerbPrototype : IPrototype, IInheritingPrototype
{
    /// <inheritdoc />
    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<InteractionVerbPrototype>))]
    public string[]? Parents { get; }

    /// <inheritdoc />
    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; }

    [IdDataField]
    public string ID { get; } = default!;

    // Locale getters
    public string Name => Loc.TryGetString($"interaction-{ID}-name", out var loc) ? loc : ID;

    public string? Description => Loc.TryGetString($"interaction-{ID}-description" , out var loc) ? loc : null;

    /// <summary>
    ///     Sprite of the icon that the user sees on the verb button.
    /// </summary>
    [DataField]
    public SpriteSpecifier? Icon;

    /// <summary>
    ///     Specifies what popups are displayed when this verb is performed successfully, or unsuccessfully.
    ///     Popups specified here are shown after the associated do-after has ended, if any.
    /// </summary>
    [DataField]
    public PopupSpecifier? SuccessPopup, FailurePopup;

    /// <summary>
    ///     Specifies what popups are displayed when a do-after for this verb is started.
    ///     This is only ever used if <see cref="Delay"/> is set to a non-zero value.
    /// </summary>
    [DataField]
    public PopupSpecifier? DelayedPopup;

    /// <summary>
    ///     The requirement of this verb.
    /// </summary>
    [DataField]
    public InteractionRequirement? Requirement = null;

    /// <summary>
    ///     The action of this verb. It defines the conditions under which this verb is shown, as well as what the verb does.
    /// </summary>
    /// <remarks>Made server-only because many actions require authoritative access to the server.</remarks>
    [DataField(serverOnly: true)]
    public InteractionAction? Action = null;

    /// <summary>
    ///     If true, this action will be hidden if the <see cref="Requirement"/> does not pass its IsMet check. Otherwise it will be shown, but disabled.
    /// </summary>
    /// <remarks>I apologize, I could not come up with a better name.</remarks>
    [DataField]
    public bool HideByRequirement = false;

    /// <summary>
    ///     If true, this action will be hidden if the <see cref="Action"/> does not pass its IsAllowed check. Otherwise it will be shown, but disabled.
    /// </summary>
    [DataField]
    public bool HideWhenInvalid = true;

    /// <summary>
    ///     The delay of the verb. Anything greater than zero constitutes a do-after.
    /// </summary>
    [DataField]
    public TimeSpan Delay = TimeSpan.Zero;

    /// <summary>
    ///     Arguments of the do-after shown if <see cref="Delay"/> is greater than zero.
    ///     The user, target, needHand, event, and other required parameters are set up automatically when the do-after is created.
    /// </summary>
    [DataField]
    public DoAfterArgs DoAfter = new DoAfterArgs()
    {
        BreakOnDamage = true,
        BreakOnTargetMove = true,
        BreakOnUserMove = true,
        BreakOnWeightlessMove = true,
        RequireCanInteract = false,
        // Never used, but must be present because the field is non-nullable and will error during serialization if not set.
        Event = new InteractionVerbDoAfterEvent(default, default!)
    };

    [DataField]
    public RangeSpecifier Range = new();

    /// <summary>
    ///     Whether this interaction implies direct body contact (transfer of fibers, fingerprints, etc).
    /// </summary>
    [DataField("contactInteraction")]
    public bool DoContactInteraction = true;

    [DataField]
    public bool RequiresHands = false;

    [DataField]
    public bool RequiresCanInteract = true;

    /// <summary>
    ///     If true, this verb can be invoked by the user on itself.
    /// </summary>
    [DataField]
    public bool AllowSelfInteract = false;

    /// <summary>
    ///     Priority of the verb. Verbs with higher priority will be shown first.
    /// </summary>
    [DataField]
    public int Priority = 0;

    /// <summary>
    ///     If true, this verb can be invoked on any entity that the action is allowed on, even if its components don't specify it.
    /// </summary>
    [DataField]
    public bool Global = false;

    [DataDefinition, Serializable]
    public partial struct RangeSpecifier()
    {
        [DataField] public float Min = 0f;
        [DataField] public float Max = float.PositiveInfinity;
        [DataField] public bool Inverse = false;

        public bool IsInRange(float value) => (Inverse ? value < Min || value > Max : value >= Min && value <= Max);
    }

    /// <summary>
    ///     Specifies how popups should be shown.<br/>
    ///     Popup locales follow the format "interaction-[verb id]-[prefix]-[kind suffix]-popup", where: <br/>
    ///     - [prefix] is <see cref="PopupPrefix"/>, which is usually one of: "success", "fail", "delayed". <br/>
    ///     - [kind suffix] is one of the respective suffix properties, typically "self", "target", or "others" <br/>
    /// </summary>
    /// <remarks>
    ///     The following parameters may be used in the locale: <br/>
    ///     - {$user} - The performer of the action. <br/>
    ///     - {$target} - The target of the action. <br/>
    ///     - {$used} - The active-hand item used in the action. May be null, then "0" is used instead.
    ///     - {$selfTarget} - A boolean value that indicates whether the action is used on the user itself.
    /// </remarks>
    [DataDefinition, Serializable]
    public partial struct PopupSpecifier()
    {
        /// <summary>
        ///     Popup loc prefix shown when the popup is shown.
        /// </summary>
        [DataField("prefix")]
        public string PopupPrefix = string.Empty;

        [DataField]
        public PopupTargetSpecifier PopupTarget = PopupTargetSpecifier.TargetThenUser;

        [DataField]
        public PopupType PopupType = PopupType.Medium;

        /// <summary>
        ///     If true, the respective success/fail popups will be logged into chat, as players perceive them.
        /// </summary>
        [DataField]
        public bool LogPopup = true;

        /// <summary>
        ///     Chat channel to which popups will be logged if <see cref="LogPopup"/> is true.
        /// </summary>
        [DataField]
        public ChatChannel LogChannel = ChatChannel.Emotes;

        /// <summary>
        ///     Color of the chat message sent if <see cref="LogPopup"/> is true. If null, defaults based on <see cref="PopupType"/>.
        /// </summary>
        [DataField]
        public Color? LogColor = null;

        [DataField("self")]
        public string? SelfSuffixField = "self";
        /// <summary>
        ///     Loc prefix for popups shown for the performer of the verb. If set to null, defaults to <see cref="OthersSuffix"/>.
        /// </summary>
        public string? SelfSuffix => SelfSuffixField ?? OthersSuffix;

        [DataField("target")]
        public string? TargetSuffixField = "target";
        /// <summary>
        ///     Loc prefix for popups shown for the target of the verb. If set to null, defaults to <see cref="OthersSuffix"/>.
        /// </summary>
        public string? TargetSuffix => TargetSuffixField ?? SelfSuffix;

        /// <summary>
        ///     Loc prefix for popups shown for other people observing the verb. This also defines what is logged into the chat if
        ///     <see cref="InteractionVerbPrototype.LogPopup"/> is true. If null, no popup will be shown for others.
        /// </summary>
        [DataField("others")]
        public string? OthersSuffix = "others";

        /// <summary>
        ///     Sound played when the popup is shown, at the location of the popup. If null, no sound will be played.
        /// </summary>
        [DataField]
        public SoundSpecifier? Sound;

        /// <summary>
        ///     If true, the sound will be perceived by everyone in the PVS of the popup.
        ///     Otherwise, it will be perceived only by the target and the user.
        /// </summary>
        [DataField]
        public bool SoundPerceivedByOthers = true;

        [DataField]
        public AudioParams SoundParams = new AudioParams()
        {
            Variation = 0.1f
        };
    }

    [Serializable]
    public enum PopupTargetSpecifier
    {
        /// <summary>
        ///     Popup will be shown above the person executing the verb.
        /// </summary>
        User,
        /// <summary>
        ///     Popup will be shown above the target of the verb.
        /// </summary>
        Target,
        /// <summary>
        ///     The user will see the popup shown above itself, others will see the popup above the target.
        /// </summary>
        UserThenTarget,
        /// <summary>
        ///     The target will see the popup shown above itself, others will see the popup above the user.
        /// </summary>
        TargetThenUser
    }
}

