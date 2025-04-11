using Content.Server.Antag;
using Content.Shared._EE.Shadowling;


namespace Content.Server._EE.Shadowling;


/// <summary>
/// This handles Thralls antag briefing
/// </summary>
public sealed class ShadowlingThrallSystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ThrallComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(EntityUid uid, ThrallComponent component, ComponentStartup args)
    {
        _antag.SendBriefing(uid, Loc.GetString("thrall-role-greeting"), Color.MediumPurple, null); // todo: find sfx

        // Handle their abilities here
    }
}
