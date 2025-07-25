using Content.Shared.Construction.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared.Construction.Prototypes;

/// <summary>
/// This is a prototype for categorizing
/// different types of machine parts.
/// </summary>
[Prototype("machinePart")]
public sealed partial class MachinePartPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// A human-readable name for the machine part type.
    /// </summary>
    [DataField(required: true)]
    public LocId Name;

    /// <summary>
    /// A stock part entity based on the machine part.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId<MachinePartComponent> StockPartPrototype = string.Empty;
}
