using Content.Shared.FootPrint;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.FixedPoint;
using Content.Shared.Fluids.Components;
using Content.Shared.Forensics;
using Robust.Shared.Physics.Events;
using Robust.Shared.Prototypes;


namespace Content.Server.FootPrint;

public sealed class PuddleFootPrintsSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _protoMan = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PuddleFootPrintsComponent, EndCollideEvent>(OnStepTrigger);
    }

    private void OnStepTrigger(EntityUid uid, PuddleFootPrintsComponent component, ref EndCollideEvent args)
    {
        if (!TryComp<PuddleComponent>(uid, out var puddle) || !TryComp<FootPrintsComponent>(args.OtherEntity, out var tripper))
            return;

        // Transfer DNAs from the puddle to the tripper
        if (TryComp<ForensicsComponent>(uid, out var puddleForensics))
        {
            tripper.DNAs.UnionWith(puddleForensics.DNAs);
            if(TryComp<ForensicsComponent>(args.OtherEntity, out var tripperForensics))
                tripperForensics.DNAs.UnionWith(puddleForensics.DNAs);
        }

        // Transfer reagents from the puddle to the tripper.
        // Ideally it should be a two-way process, but that is too hard to simulate and will have very little effect outside of potassium-water spills.
        var quantity = puddle.Solution?.Comp?.Solution?.Volume ?? 0;
        var footprintsCapacity = tripper.ContainedSolution.AvailableVolume;

        if (quantity <= 0 || footprintsCapacity <= 0)
            return;

        var transferAmount = FixedPoint2.Min(footprintsCapacity, quantity * component.SizeRatio);
        var transferred = _solutionContainer.SplitSolution(puddle.Solution!.Value, transferAmount);
        tripper.ContainedSolution.AddSolution(transferred, _protoMan);
    }
}
