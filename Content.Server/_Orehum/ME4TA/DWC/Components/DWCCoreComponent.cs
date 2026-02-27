using Robust.Shared.Prototypes;

namespace Content.Server._Orehum.ME4TA.DWCCore.Components;

[RegisterComponent]
public sealed partial class DWCCoreComponent : Component
{
    [DataField]
    public int MaxSpawns = 3;

    /// <summary>
    /// When this amount of mobs is killed, tendril breaks.
    /// </summary>
    [DataField]
    public int MobsToDefeat = 5;

    [ViewVariables]
    public int DefeatedMobs = 0;

    [DataField]
    public float SpawnDelay = 10f;

    [DataField(required: true)]
    public List<EntProtoId> Spawns = [];

    [ViewVariables]
    public List<EntityUid> Mobs = [];

    [ViewVariables(VVAccess.ReadWrite)]
    public TimeSpan LastSpawn = TimeSpan.Zero;

    [ViewVariables]
    public bool DestroyedWithMobs;
}
