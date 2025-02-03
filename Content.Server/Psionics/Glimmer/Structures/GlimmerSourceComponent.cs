namespace Content.Server.Psionics.Glimmer;

[RegisterComponent]
/// <summary>
/// Adds to glimmer at regular intervals. We'll use it for glimmer drains too when we get there.
/// </summary>
public sealed partial class GlimmerSourceComponent : Component
{
    [DataField]
    public float Accumulator = 0f;

    [DataField]
    public bool Active = true;

    /// <summary>
    ///     Since glimmer is an int, we'll do it like this.
    /// </summary>
    [DataField]
    public float SecondsPerGlimmer = 10f;

    /// <summary>
    ///     True if it produces glimmer, false if it subtracts it.
    /// </summary>
    [DataField]
    public bool AddToGlimmer = true;

    /// <summary>
    ///     If not null, this entity generates this value as a baseline number of research points per second, eg: Probers.
    ///     Actual glimmer research sources will scale with GlimmerEquilibriumRatio
    /// </summary>
    [DataField]
    public int? ResearchPointGeneration = null;

    /// <summary>
    ///     Controls whether this entity requires electrical power to generate research points.
    /// </summary>
    [DataField]
    public bool RequiresPower = true;

    /// <summary>
    ///     Above GlimmerEquilibrium, glimmer generation is increased exponentially, but has an offset to prevent things from spiralling out of control.
    ///     Increasing the offset will make this entity's exponential growth weaker, while decreasing it makes it stronger. Negative numbers are valid by the way :)
    /// </summary>
    [DataField]
    public int GlimmerExponentOffset = 0;
}
