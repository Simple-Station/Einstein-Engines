using Content.Shared.Actions;
using Content.Shared.Aliens.Components;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Robust.Shared.Network;

namespace Content.Shared.Aliens.Systems;

/// <summary>
/// This handles...
/// </summary>
public sealed class SharedAlienQueenSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AlienQueenComponent, ComponentStartup>(OnCompInit);
        SubscribeLocalEvent<AlienQueenComponent, ComponentShutdown>(OnCompRemove);

    }

    /// <summary>
    /// Giveths the action to preform making acid on the entity
    /// </summary>
    private void OnCompInit(EntityUid uid, AlienQueenComponent comp, ComponentStartup args)
    {
        _actionsSystem.AddAction(uid, ref comp.ActionEntity, comp.Action);
    }

    /// <summary>
    /// Takeths away the action to preform making acid from the entity.
    /// </summary>
    private void OnCompRemove(EntityUid uid, AlienQueenComponent comp, ComponentShutdown args)
    {
        _actionsSystem.RemoveAction(uid, comp.ActionEntity);
    }
}

