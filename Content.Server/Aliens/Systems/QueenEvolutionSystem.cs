using Content.Server.Aliens.Components;
using Content.Server.Popups;
using Content.Shared.Actions;
using Content.Shared.Aliens.Components;
using Content.Shared.Aliens.Systems;

namespace Content.Server.Aliens.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class QueenEvolutionSystem : EntitySystem
{
    /// <inheritdoc/>
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly AlienEvolutionSystem _alienEvolution = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<QueenEvolutionComponent, ComponentInit>(OnComponentInit);

        SubscribeLocalEvent<QueenEvolutionComponent, AlienQueenEvolveActionEvent>(OnEvolveQueen);
    }

    private void OnComponentInit(EntityUid uid, QueenEvolutionComponent component, ComponentInit args)
    {

        _actionsSystem.AddAction(uid, ref component.QueenEvolutionActionEntity, component.QueenEvolutionAction, uid);
    }

    private void OnEvolveQueen(EntityUid uid, QueenEvolutionComponent component, AlienQueenEvolveActionEvent args)
    {
        if (TryComp<PlasmaVesselComponent>(uid, out var plasmaComp)
            && plasmaComp.Plasma <= component.PlasmaCost)
        {
            _popup.PopupEntity(Loc.GetString("alien-action-fail-plasma"), uid, uid);
            return;
        }

        if (EntityQueryEnumerator<AlienQueenComponent>().MoveNext(out _, out _))
        {
            _popup.PopupEntity(Loc.GetString("alien-evolution-fail"), uid, uid);
            return;
        }

        _alienEvolution.Evolve(uid, component.QueenPolymorphPrototype);
    }

}
