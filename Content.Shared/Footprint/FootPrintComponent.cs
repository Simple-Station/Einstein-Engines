using Content.Shared.Chemistry.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.FootPrint;

/// <summary>
///     This is used for marking footsteps, handling footprint drawing.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class FootPrintComponent : Component
{
    /// <summary>
    ///     Owner (with <see cref="FootPrintsComponent"/>) of a print (this component).
    /// </summary>
    public EntityUid PrintOwner;

    [DataField]
    public string SolutionName = "step";

    [ViewVariables]
    public Entity<SolutionComponent>? Solution;
}

[Serializable, NetSerializable]
public sealed class FootPrintState(NetEntity netEntity) : ComponentState
{
    public NetEntity PrintOwner { get; private set; } = netEntity;
}
