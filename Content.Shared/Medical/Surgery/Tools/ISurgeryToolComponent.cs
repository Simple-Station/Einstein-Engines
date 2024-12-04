namespace Content.Shared.Medical.Surgery.Tools;

public interface ISurgeryToolComponent
{
    [DataField]
    public string ToolName { get; }

    // Mostly intended for discardable or non-reusable tools.
    [DataField]
    public bool? Used { get; set; }

    /// <summary>
    ///     Divide the doafter's duration by this value.
    ///     This is per-type so you can have something that's a good scalpel but a bad retractor.
    /// </summary>
    [DataField]
    public float Speed { get; set; }
}
