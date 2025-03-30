using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;

namespace Content.Shared.Physics;

public sealed class FrictionRemoverSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhysicsComponent, PhysicsSleepEvent>(RemoveDampening);
    }


    private void RemoveDampening(EntityUid uid, PhysicsComponent component, PhysicsSleepEvent args)
    {
        var linear = 0f;
        var angular = 0f;
        if (TryComp<PassiveDampeningComponent>(uid, out var dampening) && dampening.Enabled)
        {
            // use passive, that said, if its already set, it may have been updated by shuttle console.
            // don't overwrite shuttle console just because you start moving or stop
            // because for some reason when you go from stopped to moving, or moving to stopped this method is called
            linear = component.LinearDamping != 0 ? component.LinearDamping : dampening.LinearDampening;
            angular = component.AngularDamping != 0 ? component.AngularDamping: dampening.AngularDampening;
        }

        _physics.SetAngularDamping(uid, component, angular, false);
        _physics.SetLinearDamping(uid, component, linear);
    }
}
