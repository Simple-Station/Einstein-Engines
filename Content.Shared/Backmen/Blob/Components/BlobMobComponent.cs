using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;

<<<<<<<< HEAD:Content.Server/Blob/BlobMobComponent.cs
namespace Content.Server.Blob;
|||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/Components/BlobMobComponent.cs
namespace Content.Server.Backmen.Blob.Components;
========
namespace Content.Shared.Backmen.Blob.Components;
>>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobMobComponent.cs

<<<<<<<< HEAD:Content.Server/Blob/BlobMobComponent.cs
[RegisterComponent]
public sealed class BlobMobComponent : Component
|||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/Components/BlobMobComponent.cs
[RegisterComponent]
public sealed partial class BlobMobComponent : Component
========
[RegisterComponent, NetworkedComponent]
public sealed partial class BlobMobComponent : Component
>>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobMobComponent.cs
{
    [ViewVariables(VVAccess.ReadOnly), DataField("healthOfPulse")]
    public DamageSpecifier HealthOfPulse = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Blunt", -4 },
            { "Slash", -4 },
            { "Piercing", -4 },
            { "Heat", -4 },
            { "Cold", -4 },
            { "Shock", -4 },
        }
    };
}
