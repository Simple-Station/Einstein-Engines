using Content.Server.Actions;
using Content.Server.Antag;
using Content.Shared._EE.Shadowling;
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

    }

    private void OnStartup(EntityUid uid, ThrallComponent component, ComponentStartup args)
    {
        _antag.SendBriefing(uid, Loc.GetString("thrall-role-greeting"), Color.MediumPurple, null); // todo: find sfx


        var nightVision = EnsureComp<NightVisionComponent>(uid);
        nightVision.ToggleAction = "ActionThrallDarksight"; // todo: not sure if this is needed, need to test it without it

        // Remove the night vision action because thrall darksight does the same thing, so why have 2 actions
        if (nightVision.ToggleActionEntity is null)
            return;
        _actions.RemoveAction(nightVision.ToggleActionEntity.Value);

        // Add Thrall Abilities
        foreach (var actionId in component.BaseThrallActions)
            _actions.AddAction(uid, actionId);
    }
}
