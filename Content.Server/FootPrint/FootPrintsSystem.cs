using System.Linq;
using Content.Server.Atmos.Components;
using Content.Shared.Inventory;
using Content.Shared.FootPrint;
using Content.Shared.Standing;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Forensics;
using Robust.Shared.Map;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.FootPrint;


public sealed class FootPrintsSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IPrototypeManager _protoMan = default!;

    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;

    private EntityQuery<AppearanceComponent> _appearanceQuery;
    private EntityQuery<StandingStateComponent> _standingStateQuery;

    public override void Initialize()
    {
        base.Initialize();

        _appearanceQuery = GetEntityQuery<AppearanceComponent>();
        _standingStateQuery = GetEntityQuery<StandingStateComponent>();

        SubscribeLocalEvent<FootPrintsComponent, ComponentStartup>(OnStartupComponent);
        SubscribeLocalEvent<FootPrintsComponent, MoveEvent>(OnMove);
    }

    private void OnStartupComponent(EntityUid uid, FootPrintsComponent component, ComponentStartup args)
    {
        component.StepSize = Math.Max(0f, component.StepSize + _random.NextFloat(-0.05f, 0.05f));
    }

    private void OnMove(EntityUid uid, FootPrintsComponent component, ref MoveEvent args)
    {
        if (component.ContainedSolution.Volume <= 0
            || TryComp<PhysicsComponent>(uid, out var physics) && physics.BodyStatus != BodyStatus.OnGround
            || args.Entity.Comp1.GridUid is not {} gridUid)
            return;

        var newPos = _transform.ToMapCoordinates(args.NewPosition).Position;
        var dragging = _standingStateQuery.TryComp(uid, out var standing) && standing.CurrentState == StandingState.Lying;
        var distance = (newPos - component.LastStepPos).Length();
        var stepSize = dragging ? component.DragSize : component.StepSize;

        if (distance < stepSize)
            return;

        // are we on a puddle? we exit, ideally we would exchange liquid and DNA with the puddle but meh, too lazy to do that now.
        var entities = _lookup.GetEntitiesIntersecting(uid, LookupFlags.All);
        if (entities.Any(HasComp<PuddleFootPrintsComponent>))
            return;

        // Spawn the footprint
        var footprintUid = Spawn(component.StepProtoId, CalcCoords(gridUid, component, args.Component, dragging));
        var stepTransform = Transform(footprintUid);
        var footPrintComponent = EnsureComp<FootPrintComponent>(footprintUid);

        // transfer owner DNA into the footsteps
        var forensics = EntityManager.EnsureComponent<ForensicsComponent>(footprintUid);
        if (TryComp<ForensicsComponent>(uid, out var ownerForensics))
            forensics.DNAs.UnionWith(ownerForensics.DNAs);

        footPrintComponent.PrintOwner = uid;
        Dirty(footprintUid, footPrintComponent);

        if (_appearanceQuery.TryComp(footprintUid, out var appearance))
        {
            var color = component.ContainedSolution.GetColor(_protoMan);
            color.A = Math.Max(0.3f, component.ContainedSolution.FillFraction);

            _appearance.SetData(footprintUid, FootPrintVisualState.State, PickState(uid, dragging), appearance);
            _appearance.SetData(footprintUid, FootPrintVisualState.Color, color, appearance);
        }

        stepTransform.LocalRotation = dragging
            ? (newPos - component.LastStepPos).ToAngle() + Angle.FromDegrees(-90f)
            : args.Component.LocalRotation + Angle.FromDegrees(180f);

        if (!TryComp<SolutionContainerManagerComponent>(footprintUid, out var solutionContainer)
            || !_solution.ResolveSolution((footprintUid, solutionContainer), footPrintComponent.SolutionName, ref footPrintComponent.Solution, out var solution))
            return;

        // Transfer from the component to the footprint
        var removedReagents = component.ContainedSolution.SplitSolution(component.FootprintVolume);
        _solution.ForceAddSolution(footPrintComponent.Solution.Value, removedReagents);

        component.RightStep = !component.RightStep;
        component.LastStepPos = newPos;
    }

    private EntityCoordinates CalcCoords(EntityUid uid, FootPrintsComponent component, TransformComponent transform, bool state)
    {
        if (state)
            return new(uid, transform.LocalPosition);

        var offset = component.RightStep
            ? new Angle(Angle.FromDegrees(180f) + transform.LocalRotation).RotateVec(component.OffsetPrint)
            : new Angle(transform.LocalRotation).RotateVec(component.OffsetPrint);

        return new(uid, transform.LocalPosition + offset);
    }

    private FootPrintVisuals PickState(EntityUid uid, bool dragging)
    {
        var state = FootPrintVisuals.BareFootPrint;

        if (_inventory.TryGetSlotEntity(uid, "shoes", out _))
            state = FootPrintVisuals.ShoesPrint;

        if (_inventory.TryGetSlotEntity(uid, "outerClothing", out var suit) && TryComp<PressureProtectionComponent>(suit, out _))
            state = FootPrintVisuals.SuitPrint;

        if (dragging)
            state = FootPrintVisuals.Dragging;

        return state;
    }
}
