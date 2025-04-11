using Content.Shared._EE.Xelthia;
using Content.Shared.Abilities.Psionics;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;
using Content.Shared.Psionics;
using Content.Shared.Bed.Sleep;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Shadowkin;
using Content.Shared.Rejuvenate;
using Content.Shared.Alert;
using Content.Shared.Rounding;
using Content.Shared.Actions;
using Robust.Shared.Prototypes;
using Content.Shared.Examine;

namespace Content.Shared._EE.Xelthia;


/// <summary>
/// This *should* make the xelthia regen ability thing show, once I finish it.
/// </summary>
public sealed class XelthiaSystem : EntitySystem
{
    /// <inheritdoc/>

    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;

    public const string XelthiaRegenerateActionId = "XelthiaRegenerateAction";
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<XelthiaComponent, ComponentStartup>(OnInit);
    }

    private void OnInit(EntityUid uid, XelthiaComponent component, ComponentStartup args)
    {
        _actionsSystem.AddAction(uid, ref component.XelthiaRegenerateAction, XelthiaRegenerateActionId, uid);

//        if (TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
//        {
//            component.LArmBackspikesColor = humanoid.EyeColor; //Yeah I'm stumped. Eye color is just used to check if I
//           component.RArmBackspikesColor = humanoid.EyeColor; //can figure out how to set these to SOME color thing here
//        } // This spat out #000000 instead of the actual eye color?? So this doesn't go here with init stuff.
    }
}
