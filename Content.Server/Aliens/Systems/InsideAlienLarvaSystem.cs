using Content.Server.Body.Systems;
using Content.Server.Polymorph.Systems;
using Content.Shared.Actions;
using Content.Shared.Aliens.Components;
using Robust.Server.Containers;

namespace Content.Server.Aliens.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class InsideAlienLarvaSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly PolymorphSystem _polymorphSystem = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly EntityManager _entityManager = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<InsideAlienLarvaComponent, ComponentInit>(OnComponentInit);

        SubscribeLocalEvent<InsideAlienLarvaComponent, AlienLarvaGrowActionEvent>(OnGrow);
    }

    private void OnComponentInit(EntityUid uid, InsideAlienLarvaComponent component, ComponentInit args)
    {
        _actionsSystem.AddAction(uid, ref component.EvolutionActionEntity, component.EvolutionAction, uid);

        _actionsSystem.SetCooldown(component.EvolutionActionEntity, component.EvolutionCooldown);
    }

    public void OnGrow(EntityUid uid, InsideAlienLarvaComponent component, AlienLarvaGrowActionEvent args)
    {
        component.IsGrown = true;
        _polymorphSystem.PolymorphEntity(uid, component.PolymorphPrototype);
    }
}
