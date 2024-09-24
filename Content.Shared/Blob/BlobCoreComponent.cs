using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared.Blob;

[RegisterComponent]
public sealed class BlobCoreComponent : Component
{
<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [DataField("antagBlobPrototypeId", customTypeSerializer: typeof(PrototypeIdSerializer<AntagPrototype>))]
    public string AntagBlobPrototypeId = "Blob";
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [DataField("antagBlobPrototypeId")]
    public ProtoId<AntagPrototype> AntagBlobPrototypeId = "Blob";
=======
    #region Live Data
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("attackRate")]
    public float AttackRate = 0.8f;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("attackRate"), AutoNetworkedField]
    public FixedPoint2 AttackRate = 0.8f;
=======
    [ViewVariables]
    public EntityUid? Observer = default!;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("returnResourceOnRemove")]
    public float ReturnResourceOnRemove = 0.3f;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("returnResourceOnRemove"), AutoNetworkedField]
    public FixedPoint2 ReturnResourceOnRemove = 0.3f;
=======
    [ViewVariables]
    public HashSet<EntityUid> BlobTiles = [];
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("canSplit")]
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("canSplit"), AutoNetworkedField]
=======
    [ViewVariables]
    public List<EntityUid> Actions = [];

    [ViewVariables]
    public TimeSpan NextAction = TimeSpan.Zero;

    [ViewVariables]
    public BlobChemType CurrentChem = BlobChemType.ReactiveSpines;

    #endregion

    #region Balance

    [DataField]
    public FixedPoint2 CoreBlobTotalHealth = 400;

    [DataField]
    public float AttackRate = 0.6f;

    [DataField]
    public float GrowRate = 0.4f;

    [DataField]
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    public bool CanSplit = true;

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [DataField("attackSound")]
    public SoundSpecifier AttackSound = new SoundPathSpecifier("/Audio/Animals/Blob/blobattack.ogg");
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [DataField("attackSound"), AutoNetworkedField]
    public SoundSpecifier AttackSound = new SoundPathSpecifier("/Audio/Animals/Blob/blobattack.ogg");
=======
    #endregion

    #region Damage Specifiers
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite)]
    public Dictionary<BlobChemType, DamageSpecifier> ChemDamageDict { get; set; } = new()
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public Dictionary<BlobChemType, DamageSpecifier> ChemDamageDict { get; set; } = new()
=======
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public BlobChemDamage ChemDamageDict { get; set; } = new()
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    {
        {
            BlobChemType.BlazingOil, new DamageSpecifier()
            {
                DamageDict = new Dictionary<string, FixedPoint2>
                {
                    { "Heat", 15 },
                    { "Structural", 150 },
                }
            }
        },
        {
            BlobChemType.ReactiveSpines, new DamageSpecifier()
            {
                DamageDict = new Dictionary<string, FixedPoint2>
                {
                    { "Blunt", 8 },
                    { "Slash", 8 },
                    { "Piercing", 8 },
                    { "Structural", 150 },
                }
            }
        },
        {
            BlobChemType.ExplosiveLattice, new DamageSpecifier()
            {
                DamageDict = new Dictionary<string, FixedPoint2>
                {
                    { "Heat", 5 },
                    { "Structural", 150 },
                }
            }
        },
        {
            BlobChemType.ElectromagneticWeb, new DamageSpecifier()
            {
                DamageDict = new Dictionary<string, FixedPoint2>
                {
                    { "Structural", 150 },
                    { "Heat", 20 },
                },
            }
        },
        {
            BlobChemType.RegenerativeMateria, new DamageSpecifier()
            {
                DamageDict = new Dictionary<string, FixedPoint2>
                {
                    { "Structural", 150 },
                    { "Poison", 15 },
                }
            }
        },
    };

    #endregion

    #region Blob Chems

    [ViewVariables]
    public readonly BlobChemColors ChemСolors = new()
    {
        {BlobChemType.ReactiveSpines, Color.FromHex("#637b19")},
        {BlobChemType.BlazingOil, Color.FromHex("#937000")},
        {BlobChemType.RegenerativeMateria, Color.FromHex("#441e59")},
        {BlobChemType.ExplosiveLattice, Color.FromHex("#6e1900")},
        {BlobChemType.ElectromagneticWeb, Color.FromHex("#0d7777")},
    };

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadOnly), DataField("blobExplosive")]
    public string BlobExplosive = "Blob";

    [ViewVariables(VVAccess.ReadOnly), DataField("defaultChem")]
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadOnly), DataField("blobExplosive"), AutoNetworkedField]
    public ProtoId<ExplosionPrototype> BlobExplosive = "Blob";

    [ViewVariables(VVAccess.ReadOnly), DataField("defaultChem"), AutoNetworkedField]
=======
    [DataField]
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    public BlobChemType DefaultChem = BlobChemType.ReactiveSpines;

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadOnly), DataField("currentChem")]
    public BlobChemType CurrentChem = BlobChemType.ReactiveSpines;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadOnly), DataField("currentChem"), AutoNetworkedField]
    public BlobChemType CurrentChem = BlobChemType.ReactiveSpines;
=======
    #endregion
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("factoryRadiusLimit")]
    public float FactoryRadiusLimit = 6f;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("factoryRadiusLimit"), AutoNetworkedField]
    public float FactoryRadiusLimit = 6f;
=======
    #region Blob Costs
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("resourceRadiusLimit")]
    public float ResourceRadiusLimit = 3f;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("resourceRadiusLimit"), AutoNetworkedField]
    public float ResourceRadiusLimit = 3f;
=======
    [DataField]
    public int ResourceBlobsTotal;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("nodeRadiusLimit")]
    public float NodeRadiusLimit = 4f;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("nodeRadiusLimit"), AutoNetworkedField]
    public float NodeRadiusLimit = 4f;
=======
    [DataField]
    public FixedPoint2 AttackCost = 4;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("attackCost")]
    public FixedPoint2 AttackCost = 2;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("attackCost"), AutoNetworkedField]
    public FixedPoint2 AttackCost = 2;
=======
    [DataField]
    public BlobTileCosts BlobTileCosts = new()
    {
        {BlobTileType.Core, 0},
        {BlobTileType.Invalid, 0},
        {BlobTileType.Resource, 60},
        {BlobTileType.Factory, 80},
        {BlobTileType.Node, 50},
        {BlobTileType.Reflective, 15},
        {BlobTileType.Strong, 15},
        {BlobTileType.Normal, 6},
        /*
        {BlobTileType.Storage, 50},
        {BlobTileType.Turret, 75},*/
    };
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("factoryBlobCost")]
    public FixedPoint2 FactoryBlobCost = 60;

    [ViewVariables(VVAccess.ReadWrite), DataField("normalBlobCost")]
    public FixedPoint2 NormalBlobCost = 4;

    [ViewVariables(VVAccess.ReadWrite), DataField("resourceBlobCost")]
    public FixedPoint2 ResourceBlobCost = 40;

    [ViewVariables(VVAccess.ReadWrite), DataField("nodeBlobCost")]
    public FixedPoint2 NodeBlobCost = 50;

    [ViewVariables(VVAccess.ReadWrite), DataField("blobbernautCost")]
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("factoryBlobCost"), AutoNetworkedField]
    public FixedPoint2 FactoryBlobCost = 60;

    [ViewVariables(VVAccess.ReadWrite), DataField("normalBlobCost"), AutoNetworkedField]
    public FixedPoint2 NormalBlobCost = 4;

    [ViewVariables(VVAccess.ReadWrite), DataField("resourceBlobCost"), AutoNetworkedField]
    public FixedPoint2 ResourceBlobCost = 40;

    [ViewVariables(VVAccess.ReadWrite), DataField("nodeBlobCost"), AutoNetworkedField]
    public FixedPoint2 NodeBlobCost = 50;

    [ViewVariables(VVAccess.ReadWrite), DataField("blobbernautCost"), AutoNetworkedField]
=======
    [DataField]
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    public FixedPoint2 BlobbernautCost = 60;

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("strongBlobCost")]
    public FixedPoint2 StrongBlobCost = 15;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("strongBlobCost"), AutoNetworkedField]
    public FixedPoint2 StrongBlobCost = 15;
=======
    [DataField]
    public FixedPoint2 SplitCoreCost = 400;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("reflectiveBlobCost")]
    public FixedPoint2 ReflectiveBlobCost = 15;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("reflectiveBlobCost"), AutoNetworkedField]
    public FixedPoint2 ReflectiveBlobCost = 15;
=======
    [DataField]
    public FixedPoint2 SwapCoreCost = 200;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("splitCoreCost")]
    public FixedPoint2 SplitCoreCost = 100;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("splitCoreCost"), AutoNetworkedField]
    public FixedPoint2 SplitCoreCost = 100;
=======
    [DataField]
    public FixedPoint2 SwapChemCost = 70;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("swapCoreCost")]
    public FixedPoint2 SwapCoreCost = 80;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("swapCoreCost"), AutoNetworkedField]
    public FixedPoint2 SwapCoreCost = 80;
=======
    #endregion
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("swapChemCost")]
    public FixedPoint2 SwapChemCost = 40;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("swapChemCost"), AutoNetworkedField]
    public FixedPoint2 SwapChemCost = 40;
=======
    #region Blob Ranges
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("reflectiveBlobTile")]
    public string ReflectiveBlobTile = "ReflectiveBlobTile";
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("reflectiveBlobTile"), AutoNetworkedField]
    public EntProtoId<BlobTileComponent> ReflectiveBlobTile = "ReflectiveBlobTile";
=======
    [DataField]
    public float NodeRadiusLimit = 5f;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("strongBlobTile")]
    public string StrongBlobTile = "StrongBlobTile";
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("strongBlobTile"), AutoNetworkedField]
    public EntProtoId<BlobTileComponent> StrongBlobTile = "StrongBlobTile";
=======
    [DataField]
    public float TilesRadiusLimit = 9f;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("normalBlobTile")]
    public string NormalBlobTile = "NormalBlobTile";
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("normalBlobTile"), AutoNetworkedField]
    public EntProtoId<BlobTileComponent> NormalBlobTile = "NormalBlobTile";
=======
    #endregion
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("factoryBlobTile")]
    public string FactoryBlobTile = "FactoryBlobTile";
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("factoryBlobTile"), AutoNetworkedField]
    public EntProtoId FactoryBlobTile = "FactoryBlobTile";
=======
    #region Prototypes
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("resourceBlobTile")]
    public string ResourceBlobTile = "ResourceBlobTile";
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("resourceBlobTile"), AutoNetworkedField]
    public EntProtoId<BlobTileComponent> ResourceBlobTile = "ResourceBlobTile";
=======
    [DataField]
    public BlobTileProto TilePrototypes = new()
    {
        {BlobTileType.Resource, "ResourceBlobTile"},
        {BlobTileType.Factory, "FactoryBlobTile"},
        {BlobTileType.Node, "NodeBlobTile"},
        {BlobTileType.Reflective, "ReflectiveBlobTile"},
        {BlobTileType.Strong, "StrongBlobTile"},
        {BlobTileType.Normal, "NormalBlobTile"},
        {BlobTileType.Invalid, "NormalBlobTile"}, // wtf
        //{BlobTileType.Storage, "StorageBlobTile"},
        //{BlobTileType.Turret, "TurretBlobTile"},
        {BlobTileType.Core, "CoreBlobTile"},
    };
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("nodeBlobTile")]
    public string NodeBlobTile = "NodeBlobTile";
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("nodeBlobTile"), AutoNetworkedField]
    public string NodeBlobTile = "NodeBlobTile";
=======
    [DataField(required: true)]
    public List<ProtoId<EntityPrototype>> ActionPrototypes = [];
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("coreBlobTile")]
    public string CoreBlobTile = "CoreBlobTileGhostRole";
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("coreBlobTile"), AutoNetworkedField]
    public string CoreBlobTile = "CoreBlobTileGhostRole";
=======
    [DataField]
    public ProtoId<ExplosionPrototype> BlobExplosive = "Blob";
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("coreBlobTotalHealth")]
    public FixedPoint2 CoreBlobTotalHealth = 400;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite), DataField("coreBlobTotalHealth"), AutoNetworkedField]
    public FixedPoint2 CoreBlobTotalHealth = 400;
=======
    [DataField]
    public EntProtoId<BlobObserverComponent> ObserverBlobPrototype = "MobObserverBlob";
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite),
     DataField("ghostPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string ObserverBlobPrototype = "MobObserverBlob";
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadWrite),
     DataField("ghostPrototype", customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>)), AutoNetworkedField]
    public string ObserverBlobPrototype = "MobObserverBlob";
=======
    [DataField]
    public ProtoId<AntagPrototype> AntagBlobPrototypeId = "Blob";
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [DataField("greetSoundNotification")]
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [DataField("greetSoundNotification"), AutoNetworkedField]
=======
    #endregion

    #region Sounds

    [DataField]
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    public SoundSpecifier GreetSoundNotification = new SoundPathSpecifier("/Audio/Effects/clang.ogg");

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadOnly)]
    public EntityUid? Observer = default!;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadOnly), AutoNetworkedField]
    public EntityUid? Observer = default!;
=======
    [DataField]
    public SoundSpecifier AttackSound = new SoundPathSpecifier("/Audio/Animals/Blob/blobattack.ogg");
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs

<<<<<<< HEAD:Content.Shared/Blob/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadOnly)]
    public HashSet<EntityUid> BlobTiles = new();

    public TimeSpan NextAction = TimeSpan.Zero;

    [ViewVariables(VVAccess.ReadWrite)]
    public FixedPoint2 Points = 0;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
    [ViewVariables(VVAccess.ReadOnly)]
    public HashSet<EntityUid> BlobTiles = new();

    [AutoNetworkedField]
    public TimeSpan NextAction = TimeSpan.Zero;

    [DataField("actionHelpBlob")]
    public EntityUid? ActionHelpBlob = null;
    [DataField("actionSwapBlobChem")]
    public EntityUid? ActionSwapBlobChem = null;
    [DataField("actionTeleportBlobToCore")]
    public EntityUid? ActionTeleportBlobToCore = null;
    [DataField("actionTeleportBlobToNode")]
    public EntityUid? ActionTeleportBlobToNode = null;
    [DataField("actionCreateBlobFactory")]
    public EntityUid? ActionCreateBlobFactory = null;
    [DataField("actionCreateBlobResource")]
    public EntityUid? ActionCreateBlobResource = null;
    [DataField("actionCreateBlobNode")]
    public EntityUid? ActionCreateBlobNode = null;
    [DataField("actionCreateBlobbernaut")]
    public EntityUid? ActionCreateBlobbernaut = null;
    [DataField("actionSplitBlobCore")]
    public EntityUid? ActionSplitBlobCore = null;
    [DataField("actionSwapBlobCore")]
    public EntityUid? ActionSwapBlobCore = null;
=======
    #endregion
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Shared/Backmen/Blob/Components/BlobCoreComponent.cs
}

[Serializable, NetSerializable]
public enum BlobChemType : byte
{
    BlazingOil,
    ReactiveSpines,
    RegenerativeMateria,
    ExplosiveLattice,
    ElectromagneticWeb
}
