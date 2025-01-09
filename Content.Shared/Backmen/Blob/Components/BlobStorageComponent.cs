using Content.Shared.FixedPoint;

namespace Content.Shared.Backmen.Blob.Components;

[RegisterComponent]
public sealed partial class BlobStorageComponent : Component
{
    [DataField]
    public FixedPoint2 AddTotalStorage = 100;

    [DataField]
    public FixedPoint2 DeleteOnRemove = 60;
}
