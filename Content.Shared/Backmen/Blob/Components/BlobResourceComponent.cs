using Content.Shared.FixedPoint;

<<<<<<<< HEAD:Content.Server/Blob/BlobResourceComponent.cs
namespace Content.Server.Blob;
|||||||| parent of 3c3173a51d ([Tweak] Blob Things (#963)):Content.Server/Backmen/Blob/Components/BlobResourceComponent.cs
namespace Content.Server.Backmen.Blob.Components;
========
namespace Content.Shared.Backmen.Blob.Components;
>>>>>>>> 3c3173a51d ([Tweak] Blob Things (#963)):Content.Shared/Backmen/Blob/Components/BlobResourceComponent.cs

[RegisterComponent]
public sealed class BlobResourceComponent : Component
{
    [DataField]
    public FixedPoint2 PointsPerPulsed = 3;
}
