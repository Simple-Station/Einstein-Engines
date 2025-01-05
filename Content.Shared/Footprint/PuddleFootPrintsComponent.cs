using Content.Shared.FixedPoint;


namespace Content.Shared.FootPrint;

[RegisterComponent]
public sealed partial class PuddleFootPrintsComponent : Component
{
    /// <summary>
    ///     Ratio between puddle volume and the amount of reagents that can be transferred from it.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 SizeRatio = 0.15f;
}
