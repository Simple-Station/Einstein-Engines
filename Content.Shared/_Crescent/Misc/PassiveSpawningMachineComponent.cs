using Content.Shared.EntityList;
using Robust.Shared.Prototypes;


namespace Content.Shared._Crescent.Misc;


/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent]
public sealed partial class PassiveSpawningMachineComponent : Component
{
    [DataField]
    public ProtoId<EntityListPrototype> entityListProto = default!;

    [DataField]
    public float spawnDelay = 30f;

    [DataField]
    public bool requirePower = true;

    public float passedTime = 0f;
}
