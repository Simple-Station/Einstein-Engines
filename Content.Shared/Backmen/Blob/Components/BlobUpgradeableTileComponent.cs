namespace Content.Shared.Backmen.Blob.Components;

[RegisterComponent]
public sealed partial class BlobUpgradeableTileComponent : Component
{
    [DataField]
    public BlobTileType TransformTo = BlobTileType.Invalid;

    [DataField]
    public LocId Locale = "error";
}
