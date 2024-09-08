using Content.Server.Body.Components;
using Content.Server.Chemistry.Containers.EntitySystems;
using Content.Shared.CCVar;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Contests;
using Content.Shared.HeightAdjust;
using Robust.Shared.Configuration;

namespace Content.Server.HeightAdjust;

public sealed class BloodstreamAdjustSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _config = default!;
    [Dependency] private readonly ContestsSystem _contests = default!;
    [Dependency] private readonly SolutionContainerSystem _solutionContainer = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<BloodstreamAffectedByMassComponent, MapInitEvent>((uid, comp, _) => TryAdjustBloodstream((uid, comp)));
        SubscribeLocalEvent<BloodstreamAffectedByMassComponent, HeightAdjustedEvent>((uid, comp, _) => TryAdjustBloodstream((uid, comp)));
    }

    /// <summary>
    ///     Adjusts the bloodstream of the specified entity based on the settings provided by the component.
    /// </summary>
    public bool TryAdjustBloodstream(Entity<BloodstreamAffectedByMassComponent> ent)
    {
        if (!TryComp<BloodstreamComponent>(ent, out var bloodstream)
            || !_solutionContainer.TryGetSolution(ent.Owner, bloodstream.BloodSolutionName, out var bloodSolutionEnt)
            || !_config.GetCVar(CCVars.HeightAdjustModifiesBloodstream))
            return false;

        var bloodSolution = bloodSolutionEnt.Value.Comp.Solution;

        var factor = Math.Pow(_contests.MassContest(ent, bypassClamp: true, rangeFactor: 4f), ent.Comp.Power);
        factor = Math.Clamp(factor, ent.Comp.Min, ent.Comp.Max);

        var newVolume = bloodstream.BloodMaxVolume * factor;
        var newBloodLevel = bloodSolution.FillFraction * newVolume;
        bloodSolution.MaxVolume = newVolume;
        bloodSolution.SetContents([new ReagentQuantity(bloodstream.BloodReagent, newBloodLevel, null)], false);

        return true;
    }
}
