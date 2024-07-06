using Content.Server.Ghost.Roles.Raffles;
using Content.Server.Spawners.Components;
using Content.Shared.Storage;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Array;

namespace Content.Server.Backmen.SpecForces;

[Prototype("specForceTeam")]
public sealed partial class SpecForceTeamPrototype : IPrototype, IInheritingPrototype
{
    /// <summary>
    /// Name of the SpecForceTeam that will be shown at the round end manifest.
    /// </summary>
    [ViewVariables]
    [ParentDataField(typeof(AbstractPrototypeIdArraySerializer<SpecForceTeamPrototype>))]
    public string[]? Parents { get; }

    /// <summary>
    /// Is that SpecForceTeam is abstract.
    /// </summary>
    [ViewVariables]
    [NeverPushInheritance]
    [AbstractDataField]
    public bool Abstract { get; }

    [ViewVariables]
    [IdDataField]
    public string ID { get; } = default!;
    /// <summary>
    /// Name of the SpecForceTeam that will be shown at the round end manifest.
    /// </summary>
    [ViewVariables]
    [DataField("specForceName", required: true)]
    public LocId SpecForceName;
    /// <summary>
    /// Shuttle path for the SpecForce.
    /// </summary>
    [ViewVariables]
    [DataField("shuttlePath", required: true)]
    public string ShuttlePath = default!;
    /// <summary>
    /// Announcement text for the SpecForce.
    /// </summary>
    [ViewVariables]
    [DataField("spawnMarker", required: true)]
    public EntProtoId<SpawnPointComponent> SpawnMarker;
    /// <summary>
    /// Announcement text for the SpecForce.
    /// </summary>
    [ViewVariables]
    [DataField("announcementText")]
    public LocId? AnnouncementText;
    /// <summary>
    /// Announcement title for the SpecForce.
    /// </summary>
    [ViewVariables]
    [DataField("announcementTitle")]
    public LocId? AnnouncementTitle;
    /// <summary>
    /// Announcement sound for the SpecForce.
    /// </summary>
    [ViewVariables]
    [DataField("announcementSoundPath")]
    public SoundSpecifier? AnnouncementSoundPath;
    /// <summary>
    /// На какое количество игроков будет приходиться спавн ещё одной гост роли.
    /// По умолчанию: за каждого 10-го игрока прибавляется 1 гост роль
    /// </summary>
    [ViewVariables]
    [DataField("spawnPerPlayers")]
    public int SpawnPerPlayers = 10;
    /// <summary>
    /// Max amount of ghost roles that can be spawned.
    /// </summary>
    [ViewVariables]
    [DataField("maxRolesAmount")]
    public int MaxRolesAmount = 8;
    /// <summary>
    /// Specifies the raffle settings to use.
    /// </summary>
    [ViewVariables]
    [DataField("raffleConfig")]
    public GhostRoleRaffleConfig RaffleConfig = new()
    {
        Settings = "default"
    };
    /// <summary>
    /// SpecForces that will be spawned no matter what.
    /// Uses EntitySpawnEntry and therefore has ability to change spawn prob.
    /// </summary>
    [ViewVariables]
    [DataField("guaranteedSpawn")]
    public List<EntitySpawnEntry> GuaranteedSpawn = new();
    /// <summary>
    /// SpecForces that will be spawned using the spawnPerPlayers variable.
    /// Ghost roles will spawn by the order they arranged in list.
    /// </summary>
    [ViewVariables]
    [DataField("specForceSpawn")]
    public List<EntitySpawnEntry> SpecForceSpawn= new();
}
