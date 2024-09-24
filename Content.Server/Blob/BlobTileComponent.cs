<<<<<<< HEAD:Content.Server/Blob/BlobTileComponent.cs
using Content.Shared.Blob;
using Content.Shared.Damage;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobTileComponent.cs
﻿using Content.Shared.Backmen.Blob;
using Content.Shared.Damage;
=======
﻿using Content.Shared.Damage;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobTileComponent.cs
using Content.Shared.FixedPoint;

namespace Content.Server.Blob;

<<<<<<< HEAD:Content.Server/Blob/BlobTileComponent.cs
[RegisterComponent]
public sealed class BlobTileComponent : SharedBlobTileComponent
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobTileComponent.cs
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class BlobTileComponent : Component
=======
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true), Serializable]
public sealed partial class BlobTileComponent : Component
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobTileComponent.cs
{
<<<<<<< HEAD:Content.Server/Blob/BlobTileComponent.cs
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Core = default!;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobTileComponent.cs
    [DataField("color"), AutoNetworkedField]
    public Color Color = Color.White;

    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid? Core = default!;
=======
    [DataField, AutoNetworkedField]
    public Color Color = Color.White;

    [ViewVariables]
    public Entity<BlobCoreComponent>? Core;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobTileComponent.cs

    [DataField]
    public bool ReturnCost = true;

    [DataField(required: true)]
    public BlobTileType BlobTileType = BlobTileType.Invalid;

    [DataField]
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

    [DataField]
    public DamageSpecifier FlashDamage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Heat", 24 },
        }
    };
}

[Serializable]
public enum BlobTileType : byte
{
    Invalid, // invalid default value 0
    Normal,
    Strong,
    Reflective,
    Resource,
    /*
    Storage,
    Turret,
    */
    Node,
    Factory,
    Core,
}
