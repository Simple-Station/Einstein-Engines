using Content.Server.Stunnable;
using Content.Shared._EE.Shadowling;
using Robust.Shared.Timing;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles Glare variable activation and stun times
/// </summary>
public sealed class ShadowlingGlareSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly StunSystem _stun = default!;
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<ShadowlingComponent>();
        while (query.MoveNext(out _, out var sling))
        {
            if (sling.ActivateGlareTimer)
            {
                // Time before the ability activates
                sling.GlareTimeBeforeEffect -= frameTime;

                if (sling.GlareTimeBeforeEffect <= 0)
                    ActivateStun(sling.GlareTarget, sling);
            }
        }
    }

    private void ActivateStun(EntityUid target, ShadowlingComponent comp)
    {
        _stun.TryStun(target, TimeSpan.FromSeconds(comp.GlareStunTime), false);
        comp.ActivateGlareTimer = false;
    }
}
