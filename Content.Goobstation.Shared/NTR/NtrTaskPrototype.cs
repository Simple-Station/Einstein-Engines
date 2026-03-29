using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.NTR;
/// <summary>
/// This is a prototype for a cargo bounty, a set of items
/// that must be sold together in a labeled container in order
/// to receive a monetary reward.
/// </summary>
[Prototype, Serializable, NetSerializable]
public sealed partial class NtrTaskPrototype : IPrototype
{
    /// <inheritdoc/>
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public EntProtoId Proto;

    /// <summary>
    /// The monetary reward for completing the bounty
    /// </summary>
    [DataField(required: true)]
    public int Reward;

    /// <summary>
    /// A description for flava purposes.
    /// </summary>
    [DataField]
    public LocId Description;

    /// <summary>
    /// The entries that must be satisfied for the ntr bounty to be complete.
    /// </summary>
    [DataField(required: true)]
    public List<NtrTaskItemEntry> Entries = new();

    /// <summary>
    /// A prefix appended to the beginning of a bounty's ID.
    /// </summary>
    [DataField]
    public string IdPrefix = "CC";

    /// <summary>
    /// Weight for random selection (higher = more frequent)
    /// </summary>
    [DataField]
    public float Weight = 1.0f;

    [DataField]
    public float Cooldown; //in seconds

    [DataField]
    public Dictionary<ProtoId<ReagentPrototype>, FixedPoint2> Reagents = new();

    [DataField]
    public string SolutionName = "drink"; // i want to end it all already

    [DataField("reagentTask")] // shitcod
    public bool IsReagentTask;

    [DataField]
    public int Penalty = 1;
}

[DataDefinition, Serializable, NetSerializable]
public partial record struct NtrTaskItemEntry()
{
    [DataField]
    public List<string> Stamps = new();

    /// <summary>
    /// A whitelist for determining what items satisfy the entry.
    /// </summary>
    [DataField(required: true)]
    public EntityWhitelist Whitelist = default!;

    /// <summary>
    /// How much of the item must be present to satisfy the entry
    /// </summary>
    [DataField]
    public int Amount = 1;

    [DataField]
    public bool InstantCompletion;

    /// <summary>
    /// A player-facing name for the item.
    /// </summary>
    [DataField]
    public LocId Name;
}

// [DataDefinition, Serializable, NetSerializable]
// public sealed partial class NtrTaskReagentEntry
// {
//     [DataField("reagent")]
//     public string Reagent = string.Empty;

//     [DataField("amount")]
//     public int Amount;
// }
