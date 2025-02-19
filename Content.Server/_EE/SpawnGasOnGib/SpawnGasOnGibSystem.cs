using Content.Server.Atmos.EntitySystems;
using Content.Server.Body.Components;

namespace Content.Server._EE.SpawnGasOnGib;

public sealed partial class SpawnGasOnGibSystem : EntitySystem
{
    [Dependency] private readonly AtmosphereSystem _atmos = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpawnGasOnGibComponent, BeingGibbedEvent>(OnBeingGibbed);
    }

    private void OnBeingGibbed(EntityUid uid, SpawnGasOnGibComponent comp, BeingGibbedEvent args)
    {
        if (_atmos.GetContainingMixture(uid, false, true) is not { } air)
            return;

        _atmos.Merge(air, comp.Gas);
    }
}
