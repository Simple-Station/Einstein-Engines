using Content.Server.Spawners.Components;
using Robust.Shared.Spawners;

namespace Content.Server.Spawners.EntitySystems;

public sealed class SpawnOnDespawnSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpawnOnDespawnComponent, TimedDespawnEvent>(OnDespawn);
    }

    private void OnDespawn(EntityUid uid, SpawnOnDespawnComponent comp, ref TimedDespawnEvent args)
    {
        var xform = Transform(uid);

        // Lavaland Change start
        if (comp.Prototype != null)
            Spawn(comp.Prototype, xform.Coordinates);
        // Lavaland Change end

        // Lavaland Change start
        // make it spawn more (without intrusion)
        foreach (var prot in comp.Prototypes)
            Spawn(prot, xform.Coordinates);
        // Lavaland Change end
    }
}
