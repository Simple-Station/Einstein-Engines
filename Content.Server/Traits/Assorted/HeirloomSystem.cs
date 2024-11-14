using System.Linq;
using Content.Shared.Mood;
using Content.Shared.Traits.Assorted.Components;

namespace Content.Server.Traits.Assorted;

public sealed class HeirloomSystem : EntitySystem
{
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        //TODO: This could probably use some optimization
        var query = EntityManager.EntityQueryEnumerator<HeirloomHaverComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            var children = RecursiveGetAllChildren(uid);
            if (!children.Any(c => c == comp.Heirloom))
                continue;
            var ev = new MoodEffectEvent(comp.Moodlet);
            RaiseLocalEvent(uid, ev);
        }

        query.Dispose();
    }

    /// A reasonable assumption
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
