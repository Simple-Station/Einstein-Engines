using System.Linq;
using Content.Shared.Inventory;
using Content.Shared.Mood;
using Content.Shared.Traits.Assorted.Components;

namespace Content.Shared.Traits.Assorted.Systems;

public sealed class HeirloomSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;


    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityManager.EntityQueryEnumerator<HeirloomHaverComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            var children = RecursiveGetAllChildren(uid);
            if (children.All(c => c != comp.Heirloom))
                continue;
            var ev = new MoodEffectEvent(comp.Moodlet);
            RaiseLocalEvent(uid, ev);
        }
        query.Dispose();
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
