using System.Linq;
using Content.Shared.Mood;
using Content.Shared.Traits.Assorted.Components;
using Robust.Shared.Timing;


namespace Content.Server.Traits.Assorted;

public sealed class HeirloomSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private const long HeirloomRepeatDuration = 60;
    private TimeSpan _nextUpdate;


    public override void Initialize()
    {
        base.Initialize();

        _nextUpdate = _gameTiming.CurTime;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_nextUpdate > _gameTiming.CurTime)
            return;

        var query = EntityManager.EntityQueryEnumerator<HeirloomHaverComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            var children = RecursiveGetAllChildren(uid);
            var moodlet = comp.Moodlet;

            if (children.Any(c => c != comp.Heirloom))
                continue;

            var ev = new MoodEffectEvent(moodlet);
            RaiseLocalEvent(uid, ev);
        }

        query.Dispose();
        _nextUpdate = _gameTiming.CurTime + TimeSpan.FromSeconds(HeirloomRepeatDuration);
    }

    private IEnumerable<EntityUid> RecursiveGetAllChildren(EntityUid uid)
    {
        var xform = Transform(uid);
        var children = xform.ChildEnumerator;
        while (children.MoveNext(out var child))
        {
            yield return child;
            foreach (var c in RecursiveGetAllChildren(child))
                yield return c;
        }
    }
}
