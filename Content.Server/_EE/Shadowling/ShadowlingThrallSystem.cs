using Content.Server.Actions;
using Content.Server.Antag;
using Content.Shared._EE.Shadowling;
using Content.Shared._EE.Shadowling.Components;
using Content.Shared._EE.Shadowling.Thrall;
using Content.Shared.Actions;
using Content.Shared.Overlays.Switchable;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles Thralls antag briefing and abilities
/// </summary>
public sealed class ShadowlingThrallSystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly ActionsSystem _actions = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThrallComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<ThrallComponent, ComponentRemove>(OnRemove);
    }

    private void OnStartup(EntityUid uid, ThrallComponent component, ComponentStartup args)
    {
        _antag.SendBriefing(uid, Loc.GetString("thrall-role-greeting"), Color.MediumPurple, null); // todo: find sfx
        // todo: add screen shader effect here too

        var nightVision = EnsureComp<NightVisionComponent>(uid);
        nightVision.ToggleAction = "ActionThrallDarksight"; // todo: not sure if this is needed, need to test it without it

        // Remove the night vision action because thrall darksight does the same thing, so why have 2 actions
        if (nightVision.ToggleActionEntity is null)
            return;
        _actions.RemoveAction(nightVision.ToggleActionEntity.Value);

        // Add Thrall Abilities
        if (!TryComp<ActionsComponent>(uid, out var actions))
            return;

        EnsureComp<ThrallGuiseComponent>(uid);
        _actions.AddAction(
            uid,
            ref component.ActionThrallDarksightEntity,
            component.ActionThrallDarksight,
            component: actions);

        _actions.AddAction(
            uid,
            ref component.ActionGuiseEntity,
            component.ActionGuise,
            component: actions);

        // todo: add comp remove event once deconversion gets added
    }

    private void OnRemove(EntityUid uid, ThrallComponent component, ComponentRemove args)
    {
        _actions.RemoveAction(component.ActionThrallDarksightEntity);
        _actions.RemoveAction(component.ActionGuiseEntity);

        RemComp<NightVisionComponent>(uid);
        RemComp<ThrallGuiseComponent>(uid);

        if (HasComp<LesserShadowlingComponent>(uid))
            RemComp<LesserShadowlingComponent>(uid);

        if (component.Converter == null)
            return;

        RaiseLocalEvent(component.Converter.Value, new ThrallRemovedEvent());
    }
}
