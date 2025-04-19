using Content.Server.Body.Systems;
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
using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Robust.Shared.Placement;
using Robust.Shared.Serialization.Manager;


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

        //IEntityManager entityManager = uid;
        IEntityManager entityManager = base.EntityManager; // There's no way this is good code. I don't know what im doing though, so. I dunno.
        var bodySystem = entityManager.System<BodySystem>();
        var transformSystem = entityManager.System<SharedTransformSystem>();

        if (!entityManager.TryGetComponent(uid, out BodyComponent? body)
            || !entityManager.TryGetComponent(uid, out TransformComponent? xform))
            return;

        var root = bodySystem.GetRootPartOrNull(uid, body);
        if (root is null)
            return;

        var parts = bodySystem.GetBodyChildrenOfType(uid, BodyPartType.Arm, body);
        foreach (var part in parts)
        {
            var partComp = part.Component;
            if (partComp.Symmetry != BodyPartSymmetry.Right)
                continue;

            foreach (var child in bodySystem.GetBodyPartChildren(part.Id, part.Component))
                entityManager.QueueDeleteEntity(child.Id);

            transformSystem.AttachToGridOrMap(part.Id);
            entityManager.QueueDeleteEntity(part.Id);

            var newLimb = entityManager.SpawnAtPosition("RightArmXelthia", xform.Coordinates);
            if (entityManager.TryGetComponent(newLimb, out BodyPartComponent? limbComp))
                bodySystem.AttachPart(root.Value.Entity, "right arm", newLimb, root.Value.BodyPart, limbComp);
        }
//        parts = bodySystem.GetBodyChildrenOfType(uid, BodyPartType.Hand, body);
//        foreach (var part in parts)
//        {
//            var partComp = part.Component;
//            if (partComp.Symmetry != BodyPartSymmetry.Right)
//                continue;
//
//            foreach (var child in bodySystem.GetBodyPartChildren(part.Id, part.Component))
//                entityManager.QueueDeleteEntity(child.Id);
//
//            transformSystem.AttachToGridOrMap(part.Id);
//            entityManager.QueueDeleteEntity(part.Id);
//
//            var newLimb = entityManager.SpawnAtPosition("RightHandXelthia", xform.Coordinates);
//            if (entityManager.TryGetComponent(newLimb, out BodyPartComponent? limbComp))
//                bodySystem.AttachPart(root.Value.Entity, "right hand", newLimb, root.Value.BodyPart, limbComp);
//        }
    }
}
