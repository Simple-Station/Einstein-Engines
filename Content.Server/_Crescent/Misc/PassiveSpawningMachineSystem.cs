using Content.Server.Power.EntitySystems;
using Content.Shared._Crescent.Misc;
using Content.Shared.EntityList;
using Content.Shared.Interaction.Events;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;


namespace Content.Server._Crescent.Misc;


/// <summary>
/// This handles...
/// </summary>
public sealed class PassiveSpawningMachineSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PowerReceiverSystem _powerReceiver = default!;
    public override void Update(float delta)
    {
        var query = EntityQueryEnumerator<PassiveSpawningMachineComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.requirePower && !_powerReceiver.IsPowered(uid))
                continue;
            comp.passedTime += delta;
            if (comp.passedTime < comp.spawnDelay)
                continue;
            comp.passedTime = 0;
            if (!_proto.TryIndex<EntityListPrototype>(comp.entityListProto, out var entityListProto))
            {
                Log.Error(
                    $"PassiveSpawningMachineSystem: EntityListProto with id {comp.entityListProto} NOT FOUND on entity prototype : {MetaData(uid).EntityPrototype}");
                continue;
            }
            var ent = _random.Pick(entityListProto.EntityIds);
            SpawnNextToOrDrop(ent, uid);
        }
    }
}
