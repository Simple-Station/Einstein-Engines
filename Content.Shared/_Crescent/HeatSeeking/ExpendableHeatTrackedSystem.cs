using Content.Server._Crescent.HeatSeeking;

namespace Content.Shared._Crescent.HeatSeeking;

public sealed class ExpendableHeatTrackedSystem : EntitySystem
{
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CanBeHeatTrackedComponent, ExpendableHeatTrackedComponent>(); // get all heat seeking missiles
        while (query.MoveNext(out var uid, out var comp, out var expendable))
        {
            if (expendable.ToggleDelay > 0f)
                expendable.ToggleDelay -= frameTime;
            else
                comp.HeatSignature = 0;
        }
    }
}
