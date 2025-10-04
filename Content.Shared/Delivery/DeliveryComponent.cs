using System.Threading;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared.Delivery;

/// <summary>
/// Component given to deliveries.
/// Means the entity is a delivery, which upon opening will grant a reward to cargo.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class DeliveryComponent : Component
{
    /// <summary>
    /// Whether this delivery has been opened before.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsOpened;

    /// <summary>
    /// Whether this delivery is still locked using the fingerprint reader.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsLocked = true;

    /// <summary>
    /// Whether it profitable to deliver this parcel to the station.
    /// </summary>
    [DataField]
    public bool IsProfitable = true;

    /// <summary>
    /// Whether this package considered fragile.
    /// </summary>
    [DataField]
    public bool IsFragile;

    /// <summary>
    /// Whether this package considered priority mail.
    /// </summary>
    [DataField]
    public bool IsPriority;

    /// <summary>
    /// The amount of spesos that gets added to the station bank account on unlock.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int SpesoReward = 500;

    /// <summary>
    /// The amount of specos that will be withdrawn from the station bank on upon destroy.
    /// </summary>
    [DataField]
    public int SpesoPenalty = -250;

    /// <summary>
    /// The name of the recipient of this delivery.
    /// Used for the examine text.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? RecipientName;

    /// <summary>
    /// The job of the recipient of this delivery.
    /// Used for the examine text.
    /// </summary>
    [DataField, AutoNetworkedField]
    public string? RecipientJobTitle;

    /// <summary>
    /// The EntityUid of the station this delivery was spawned on.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? RecipientStation;

    /// <summary>
    /// The sound to play when the delivery is unlocked.
    /// </summary>
    [DataField]
    public SoundSpecifier? UnlockSound = new SoundCollectionSpecifier("DeliveryUnlockSounds", AudioParams.Default.WithVolume(-10));

    /// <summary>
    /// The sound to play when the delivery is opened.
    /// </summary>
    [DataField]
    public SoundSpecifier? OpenSound = new SoundCollectionSpecifier("DeliveryOpenSounds");

    /// <summary>
    /// The sound to play when the delivery is opened.
    /// </summary>
    [DataField]
    public SoundSpecifier? PenaltySound = new SoundPathSpecifier("/Audio/Machines/Nuke/angry_beep.ogg");

    /// <summary>
    /// The container with all the contents of the delivery.
    /// </summary>
    [DataField]
    public string Container = "delivery";

    public CancellationTokenSource? PriorityCancelToken;
}
