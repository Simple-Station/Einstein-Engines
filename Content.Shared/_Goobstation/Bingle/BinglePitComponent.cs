using Robust.Shared.Containers;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Bingle;

[RegisterComponent]
public sealed partial class BinglePitComponent : Component
{
    /// <summary>
    /// ammount of stored
    /// </summary>
    [DataField]
    public float BinglePoints = 0f;
    [DataField]
    public float PointsForAlive = 5f;
    [DataField]
    public float AdditionalPointsForHuman = 5f;
    /// <summary>
    /// amount of Bingle Points needed for a new bingle
    /// </summary>
    [DataField]
    public float SpawnNewAt = 20f;

    /// <summary>
    /// amount bingles needed to evolve / gain a level / expand the ... THE FACTORY MUST GROW
    /// </summary>
    [DataField]
    public float MinionsMade = 0f;

    [DataField]
    public float UpgradeMinionsAfter = 10f;

    /// <summary>
    /// the Bingle pit's level
    /// </summary>
    [DataField]
    public float Level = 1f;
    /// <summary>
    /// Maximum size of the Bingle pit at this level
    /// </summary>
    [DataField]
    public float MaxSize = 3f;

    /// <summary>
    /// Where the entities go when they fall into the pit, empties when the pit is destroyed
    /// </summary>
    [DataField("pit")]
    public Container? Pit = default!;
    [DataField]
    public SoundSpecifier FallingSound = new SoundPathSpecifier("/Audio/Effects/falling.ogg");
    [DataField]
    public EntProtoId GhostRoleToSpawn = "SpawnPointGhostBingle";
}
