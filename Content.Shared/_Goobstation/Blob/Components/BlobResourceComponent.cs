using Content.Shared.FixedPoint;

namespace Content.Shared._Goobstation.Blob.Components;

[RegisterComponent]
public sealed partial class BlobResourceComponent : Component
{
    [DataField]
    public FixedPoint2 PointsPerPulsed = 3;
}
