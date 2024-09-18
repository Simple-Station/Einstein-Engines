using System.Linq;
using Content.Server.Objectives.Components;
using Content.Server.Objectives.Systems;
using Content.Server.WhiteDream.BloodCult.Gamerule;
using Content.Shared.Objectives.Components;

namespace Content.Server.WhiteDream.BloodCult.Objectives;

public sealed class KillTargetCultSystem : EntitySystem
{
    [Dependency] private readonly TargetObjectiveSystem _target = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<KillTargetCultComponent, ObjectiveAssignedEvent>(OnCultTargetAssigned);
    }

    private void OnCultTargetAssigned(Entity<KillTargetCultComponent> ent, ref ObjectiveAssignedEvent args)
    {
        // invalid prototype
        if (!TryComp<TargetObjectiveComponent>(ent.Owner, out var target))
        {
            args.Cancelled = true;
            return;
        }

        // target already assigned
        if (target.Target != null)
            return;

        var cultistRule = EntityManager.EntityQuery<BloodCultRuleComponent>().FirstOrDefault();
        if (cultistRule?.OfferingTarget is null)
        {
            return;
        }

        _target.SetTarget(ent.Owner, cultistRule.OfferingTarget.Value);
    }
}
