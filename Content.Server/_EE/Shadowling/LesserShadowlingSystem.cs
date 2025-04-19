using Content.Server.Actions;
using Content.Shared._EE.Shadowling;
using Content.Shared._EE.Shadowling.Components;
using Content.Shared._EE.Shadowling.Thrall;
using Content.Shared.Actions;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles logic for Lesser Shadowlings.
/// Just upgrades the guise ability into Shadow Walk
/// </summary>
public sealed class LesserShadowlingSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LesserShadowlingComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<LesserShadowlingComponent, ComponentRemove>(OnRemove);
    }

    private void OnStartup(EntityUid uid, LesserShadowlingComponent component, ComponentStartup args)
    {
        if (!TryComp(uid, out ActionsComponent? comp))
            return;
        // todo: remove guise once added in here and replace with shadow walk
        if (!TryComp<ThrallComponent>(uid, out var thrall))
            return;

        RemComp<LightDetectionComponent>(uid); // This was only needed for Guise
        _actions.RemoveAction(thrall.ActionGuiseEntity);

        _actions.AddAction(uid, ref component.ShadowWalkActionId, component.ShadowWalkAction, component: comp);
        EnsureComp<ShadowlingShadowWalkComponent>(uid);
    }

    private void OnRemove(EntityUid uid, LesserShadowlingComponent component, ComponentRemove args)
    {
        if (!TryComp(uid, out ActionsComponent? comp))
            return;

        _actions.RemoveAction(uid, component.ShadowWalkActionId, comp);
        RemComp<ShadowlingShadowWalkComponent>(uid);
    }
}
