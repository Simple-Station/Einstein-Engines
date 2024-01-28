using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

namespace Content.Shared.SimpleStation14.HeightAdjust;

public sealed class HeightAdjustSystem : EntitySystem
{
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedContentEyeSystem _eye = default!;


    /// <summary>
    ///     Changes the density of fixtures and zoom of eyes based on a provided float scale
    /// </summary>
    /// <param name="uid">The entity to modify values for</param>
    /// <param name="scale">The scale to multiply values by</param>
    /// <returns>True if both operations succeeded</returns>
    public bool SetScale(EntityUid uid, float scale)
    {
        var succeeded = true;
        if (EntityManager.TryGetComponent<ContentEyeComponent>(uid, out var eye))
            _eye.SetMaxZoom(uid, eye.MaxZoom * scale);
        else
            succeeded = false;

        if (EntityManager.TryGetComponent<FixturesComponent>(uid, out var fixtures))
            foreach (var fixture in fixtures.Fixtures)
                // _physics.SetDensity(uid, fixture.Key, fixture.Value, fixture.Value.Density * scale); // If you want to do the same thing without changing size
                _physics.SetRadius(uid, fixture.Key, fixture.Value, fixture.Value.Shape, fixture.Value.Shape.Radius * scale);
        else
            succeeded = false;

        return succeeded;
    }
}
