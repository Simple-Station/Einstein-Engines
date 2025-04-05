namespace Content.Shared.Atmos.Components;

// Unfortunately can't be friends yet due to magboots.
[RegisterComponent]
public sealed partial class MovedByPressureComponent : Component
{
    [DataField]
    public bool Enabled { get; set; } = true;

    [ViewVariables(VVAccess.ReadWrite)]
    public int LastHighPressureMovementAirCycle { get; set; } = 0;

    /// <summary>
    ///     Whether or not this entity is being actively moved around by pressure deltas.
    /// </summary>
    [DataField]
    public bool Throwing;

    [DataField]
    public TimeSpan ThrowingCutoffTarget;

    /// <summary>
    ///     How many seconds can this object go between being hit by space wind before it "falls to the ground".
    /// </summary>
    [DataField]
    public TimeSpan CutoffTime = TimeSpan.FromSeconds(2.0);
}

