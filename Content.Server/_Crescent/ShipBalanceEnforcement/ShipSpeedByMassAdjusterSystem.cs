using Content.Server._Crescent.Hardpoint;
using Content.Server.PointCannons;
using Content.Server.Shuttles.Components;
using Content.Shared._Crescent.ShipBalanceEnforcement;
using Robust.Server.GameObjects;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Components;

namespace Content.Server._Crescent.ShipBalanceEnforcement;

/// <summary>
/// This handles...
/// </summary>
public sealed class ShipSpeedByMassAdjusterSystem : EntitySystem
{

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ShipSpeedByMassAdjusterComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<ShipSpeedByMassAdjusterComponent, GridFixtureChangeEvent>(OnGridShape);
    }

    public void OnGridShape(EntityUid owner, ShipSpeedByMassAdjusterComponent component, ref GridFixtureChangeEvent args)
    {
        UpdateShipSpeed(owner, null, null, null);
    }
    public void OnMapInit(EntityUid owner, ShipSpeedByMassAdjusterComponent component, ref MapInitEvent args)
    {
        if (!TryComp<PhysicsComponent>(owner, out var phys))
            return;
        if (!TryComp<ShuttleComponent>(owner, out var shuttle))
            return;
        if(component.InitialGridMass == 0)
            component.InitialGridMass = phys.Mass;
        component.InitialGridSpeed = shuttle.BaseMaxLinearVelocity;
    }

    public void UpdateShipSpeed(EntityUid grid, ShuttleComponent? shuttleComp, PhysicsComponent? physComp, ShipSpeedByMassAdjusterComponent? adjusterComp)
    {
        if (!Resolve(grid, ref shuttleComp))
            return;
        if (!Resolve(grid, ref physComp))
            return;
        if (!Resolve(grid, ref adjusterComp))
            return;
        shuttleComp.BaseMaxLinearVelocity = Math.Min(
            adjusterComp.InitialGridSpeed * adjusterComp.InitialGridMass / physComp.Mass, adjusterComp.InitialGridSpeed);

    }
}
