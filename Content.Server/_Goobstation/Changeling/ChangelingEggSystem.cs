using Robust.Shared.Timing;
using Content.Shared.Changeling;
using Content.Shared.Mind;
using Content.Server.Body.Systems;
using Content.Shared.Mind.Components;

namespace Content.Server.Changeling;

public sealed class ChangelingEggSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly ChangelingSystem _changeling = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ChangelingEggComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime < comp.UpdateTimer)
                continue;

            comp.UpdateTimer = _timing.CurTime + TimeSpan.FromSeconds(comp.UpdateCooldown);

            Cycle(uid, comp);
        }
    }

    public void Cycle(EntityUid uid, ChangelingEggComponent comp)
    {
        if (comp.Active == false)
        {
            comp.Active = true;
            return;
        }

        if (TerminatingOrDeleted(comp.LingMind))
        {
            _bodySystem.GibBody(uid);
            return;
        }

        var newUid = Spawn(comp.MobToSpawn, Transform(uid).Coordinates);

        EnsureComp<MindContainerComponent>(newUid);
        _mind.TransferTo(comp.LingMind, newUid);

        EnsureComp<ChangelingComponent>(newUid);

        EntityManager.AddComponent(newUid, comp.LingStore);

        if (comp.AugmentedEyesightPurchased)
            _changeling.InitializeAugmentedEyesight(newUid);

        _bodySystem.GibBody(uid);
    }
}
