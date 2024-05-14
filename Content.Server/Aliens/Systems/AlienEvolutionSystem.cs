using Content.Server.Polymorph.Systems;
using Content.Shared.Actions;
using Content.Shared.Aliens.Components;
using Content.Shared.Aliens.Systems;
using Content.Shared.Polymorph;
using Robust.Shared.Prototypes;

namespace Content.Server.Aliens.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class AlienEvolutionSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly PolymorphSystem _polymorphSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AlienEvolutionComponent, ComponentInit>(OnComponentInit);

        SubscribeLocalEvent<AlienEvolutionComponent, AlienDroneEvolveActionEvent>(OnEvolveDrone);
        SubscribeLocalEvent<AlienEvolutionComponent, AlienSentinelEvolveActionEvent>(OnEvolveSentinel);
        SubscribeLocalEvent<AlienEvolutionComponent, AlienHunterEvolveActionEvent>(OnEvolveHunter);
    }

    private void OnComponentInit(EntityUid uid, AlienEvolutionComponent component, ComponentInit args)
    {
        _actionsSystem.AddAction(uid, ref component.DroneEvolutionActionEntity, component.DroneEvolutionAction, uid);
        _actionsSystem.AddAction(uid, ref component.SentinelEvolutionActionEntity, component.SentinelEvolutionAction, uid);
        _actionsSystem.AddAction(uid, ref component.HunterEvolutionActionEntity, component.HunterEvolutionAction, uid);

        _actionsSystem.SetCooldown(component.DroneEvolutionActionEntity, component.EvolutionCooldown);
        _actionsSystem.SetCooldown(component.SentinelEvolutionActionEntity, component.EvolutionCooldown);
        _actionsSystem.SetCooldown(component.HunterEvolutionActionEntity, component.EvolutionCooldown);
    }

    private void OnEvolveDrone(EntityUid uid, AlienEvolutionComponent component, AlienDroneEvolveActionEvent args)
    {
        Evolve(uid, component.DronePolymorphPrototype);
    }

    private void OnEvolveSentinel(EntityUid uid, AlienEvolutionComponent component, AlienSentinelEvolveActionEvent args)
    {
        Evolve(uid, component.SentinelPolymorphPrototype);
    }

    private void OnEvolveHunter(EntityUid uid, AlienEvolutionComponent component, AlienHunterEvolveActionEvent args)
    {
        Evolve(uid, component.HunterPolymorphPrototype);
    }

    public void Evolve(EntityUid uid, ProtoId<PolymorphPrototype> polymorphProtoId)
    {
        _polymorphSystem.PolymorphEntity(uid, polymorphProtoId);
    }
}


