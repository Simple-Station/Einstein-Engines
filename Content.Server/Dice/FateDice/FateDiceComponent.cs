using Content.Shared.Xenoarchaeology.XenoArtifacts;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server.Dice.FateDice;

[RegisterComponent, Access(typeof(FateDiceSystem))]
public sealed partial class FateDiceComponent : Component
{
    /// <summary>
    /// Last user that interacted with this dice
    /// </summary>
    [ViewVariables]
    public EntityUid LastUser;

    /// <summary>
    /// List of the effects that this Dice of Fate can trigger.
    /// </summary>
    [DataField("effects", customTypeSerializer: typeof(PrototypeIdListSerializer<ArtifactEffectPrototype>), required: true), ViewVariables]
    public List<string> Effects = new();

    /// <summary>
    /// The radius of the area for the boo event
    /// </summary>
    [DataField]
    public float BooRadius = 4f;

    /// <summary>
    /// The time of the entity to be deleted in seconds
    /// after it is completely been used.
    /// </summary>
    [DataField]
    public float TimeToDelete = 2f;

    /// <summary>
    /// The time in seconds to the entity effect be activated
    // after the dice is rolled.
    /// </summary>
    [DataField]
    public float TimeToActivate = 1f;

    /// <summary>
    /// How many times the dice can be rolled,
    /// if <= 0 the dice is replaced with
    /// prototype specified by ´´toReplace´´.
    /// </summary>
    [DataField]
    public int RemainingUses = 1;

    /// <summary>
    /// Prototype to replace the
    /// dice of fate when used
    /// up.
    /// </summary>
    [DataField("toReplace")]
    public string ToReplace = "Ash";

    [DataField]
    public string DeletedSound = "/Audio/Magic/blink.ogg";

    public TimeSpan? DelTime = null;
    public TimeSpan? ActTime = null;
    public int? LastRolledNumber = null;
}

