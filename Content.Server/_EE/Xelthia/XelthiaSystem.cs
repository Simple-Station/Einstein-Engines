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
using Content.Shared.Actions.Events;
using Robust.Shared.Placement;


namespace Content.Server._EE.Xelthia;


/// <summary>
/// Xelthia Arm Regrowth stuff
/// </summary>
public sealed class XelthiaSystem : EntitySystem
{
    /// <inheritdoc/>

    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public const string XelthiaRegenerateActionId = "XelthiaRegenerateAction";
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<XelthiaComponent, ComponentStartup>(OnInit);
        SubscribeLocalEvent<XelthiaComponent, ArmRegrowthEvent>(OnRegrowthAction);
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

    private void OnRegrowthAction(EntityUid uid, XelthiaComponent component, ArmRegrowthEvent args)
    {
        if (TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
        {
            component.LArmBackspikesColor = humanoid.EyeColor;
            component.RArmBackspikesColor = humanoid.EyeColor;
            // Placeholder for an actual action just so I can make sure the button. Actually does anything? Works.
        }
        // BEHOLD, COPY-PASTED AND TWEAKED CODE FROM THE SPAWN THINGS COMMAND. I HAVE NO IDEA WHAT IM DOING AND THIS IS PROBABLY ASS.
        // This currently just spawns more hands and arms without attaching them. Also they need decals. This is fucking scuffed.
        PlacementEntityEvent? placementEv = null;
        var entityCoordinates = _entityManager.GetComponent<TransformComponent>(uid).Coordinates;
        var createdEntity = _entityManager.SpawnEntity("LeftArmXelthia", entityCoordinates);
        placementEv = new PlacementEntityEvent(createdEntity, entityCoordinates, PlacementEventAction.Create, null);
        createdEntity = _entityManager.SpawnEntity("LeftHandXelthia", entityCoordinates);
        placementEv = new PlacementEntityEvent(createdEntity, entityCoordinates, PlacementEventAction.Create, null);
        createdEntity = _entityManager.SpawnEntity("RightArmXelthia", entityCoordinates);
        placementEv = new PlacementEntityEvent(createdEntity, entityCoordinates, PlacementEventAction.Create, null);
        createdEntity = _entityManager.SpawnEntity("RightHandXelthia", entityCoordinates);
        placementEv = new PlacementEntityEvent(createdEntity, entityCoordinates, PlacementEventAction.Create, null);
    }
}
