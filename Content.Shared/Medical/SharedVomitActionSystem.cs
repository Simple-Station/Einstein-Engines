using Content.Server.Medical.Components;
using Content.Shared.Actions;
using Robust.Shared.Containers;

namespace Content.Shared.Medical;

/// <summary>
/// This handles...
/// </summary>
public abstract class SharedVomitActionSystem : EntitySystem
{
    /// <inheritdoc/>

    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] protected readonly SharedContainerSystem ContainerSystem = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VomitActionComponent, MapInitEvent>(OnInit);
    }

    protected void OnInit(EntityUid uid, VomitActionComponent component, MapInitEvent args)
    {
        component.Stomach = ContainerSystem.EnsureContainer<Container>(uid, "stomach");

        _actionsSystem.AddAction(uid, ref component.VomitActionEntity, component.VomitAction);
    }


}

public sealed partial class VomitActionEvent : InstantActionEvent { }
