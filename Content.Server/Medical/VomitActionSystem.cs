using Content.Server.Body.Systems;
using Content.Server.Medical.Components;
using Content.Shared.Medical;

namespace Content.Server.Medical;

/// <summary>
/// This handles...
/// </summary>
public sealed class VomitActionSystem : SharedVomitActionSystem
{
    /// <inheritdoc/>
    [Dependency] private readonly VomitSystem _vomit = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VomitActionComponent, VomitActionEvent>(OnVomitAction);
    }

    protected void OnVomitAction(EntityUid uid, VomitActionComponent component, VomitActionEvent args)
    {
        _vomit.Vomit(uid, component.ThirstAdded, component.HungerAdded);
        ContainerSystem.EmptyContainer(component.Stomach, true);
    }
}
