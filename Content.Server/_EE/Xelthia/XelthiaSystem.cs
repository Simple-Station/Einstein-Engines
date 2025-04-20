using Content.Server.Body.Components;
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
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Placement;
using Robust.Shared.Random;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Timing;
using Content.Shared.Chemistry.Components;


namespace Content.Server._EE.Xelthia;


/// <summary>
/// Xelthia Arm Regrowth stuff
/// </summary>
public sealed class XelthiaSystem : EntitySystem
{
    /// <inheritdoc/>

    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly BloodstreamSystem _bloodstreamSystem = default!;

    //public const string XelthiaRegenerateActionId = "XelthiaRegenerateAction";
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<XelthiaComponent, ComponentStartup>(OnInit);
        SubscribeLocalEvent<XelthiaComponent, ArmRegrowthEvent>(OnRegrowthAction);
    }

    private void OnInit(EntityUid uid, XelthiaComponent component, ComponentStartup args)
    {
        _actionsSystem.AddAction(uid, ref component.XelthiaRegenerateAction, out var regenerateAction, "XelthiaRegenerateAction");
        if (regenerateAction?.UseDelay != null)
            component.UseDelay = regenerateAction.UseDelay.Value;
    }

    private void OnRegrowthAction(EntityUid uid, XelthiaComponent component, ArmRegrowthEvent args)
    {
        var start = _gameTiming.CurTime;
        var end = _gameTiming.CurTime + component.UseDelay;
        _actionsSystem.SetCooldown(component.XelthiaRegenerateAction!.Value, start, end); // Cooldown

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

        var parts = bodySystem.GetBodyChildrenOfType(uid, BodyPartType.Arm, body); // Deletes Arms and hands
        foreach (var part in parts)
        {

            foreach (var child in bodySystem.GetBodyPartChildren(part.Id, part.Component))
                entityManager.QueueDeleteEntity(child.Id);

            transformSystem.AttachToGridOrMap(part.Id);
            entityManager.QueueDeleteEntity(part.Id);
            entityManager.SpawnAtPosition("FoodMeatXelthiaTentacle", xform.Coordinates); // Should drop arms equal to the number attached
        }
        // Right Side
        var newLimb = entityManager.SpawnAtPosition("RightArmXelthia", xform.Coordinates); // Copy-Pasted Code for arm spawning
        if (entityManager.TryGetComponent(newLimb, out BodyPartComponent? limbComp))
            bodySystem.AttachPart(root.Value.Entity, "right arm", newLimb, root.Value.BodyPart, limbComp);

        var newerLimb = entityManager.SpawnAtPosition("RightHandXelthia", xform.Coordinates); // Spawns the hand
        Enum.TryParse<BodyPartType>("Hand", out var partType);
        bodySystem.TryCreatePartSlotAndAttach(newLimb, "right hand", newerLimb, partType);

        // Left Side
        newLimb = entityManager.SpawnAtPosition("LeftArmXelthia", xform.Coordinates); // Copy-Pasted Code for arm spawning
        if (entityManager.TryGetComponent(newLimb, out BodyPartComponent? limbComp2))
            bodySystem.AttachPart(root.Value.Entity, "left arm", newLimb, root.Value.BodyPart, limbComp2);

        newerLimb = entityManager.SpawnAtPosition("LeftHandXelthia", xform.Coordinates); // Spawns the hand
        bodySystem.TryCreatePartSlotAndAttach(newLimb, "left hand", newerLimb, partType);

        entityManager.EntitySysManager.GetEntitySystem<SharedAudioSystem>()
            .PlayPvs("/Audio/_EE/Voice/Xelthia/regrow.ogg", uid, null);

        var solution = new Solution();

        TryComp<BloodstreamComponent>(uid, out var stream);
        solution.AddReagent("XelthiaArmJuice", 3.5);
        _bloodstreamSystem.TryAddToChemicals(uid, solution, stream);
    }
}
