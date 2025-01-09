using Content.Server.Actions;
using Content.Server.Body.Systems;
using Content.Server.Ghost.Roles;
using Content.Server.Ghost.Roles.Components;
using Content.Server.Mind;
using Content.Shared.Backmen.Blob;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
<<<<<<< HEAD
using Content.Shared.Popups;
using Robust.Shared.Map;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703))
using Content.Shared.Popups;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
=======
using Robust.Shared.Map.Components;
using Robust.Shared.Player;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703))
using Robust.Shared.Prototypes;

namespace Content.Server.Backmen.Blob.Systems;

public sealed class BlobCarrierSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly BlobCoreSystem _blobCoreSystem = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly GhostRoleSystem _ghost = default!;
    [Dependency] private readonly BodySystem _bodySystem = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobCarrierComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<BlobCarrierComponent, TransformToBlobActionEvent>(OnTransformToBlobChanged);

        SubscribeLocalEvent<BlobCarrierComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<BlobCarrierComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<BlobCarrierComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<BlobCarrierComponent, MindRemovedMessage>(OnMindRemove);
    }


    [ValidatePrototypeId<EntityPrototype>] private const string ActionTransformToBlob = "ActionTransformToBlob";

    private void OnMindAdded(EntityUid uid, BlobCarrierComponent component, MindAddedMessage args)
    {
        component.HasMind = true;
    }

    private void OnMindRemove(EntityUid uid, BlobCarrierComponent component, MindRemovedMessage args)
    {
        component.HasMind = false;
    }

    private void OnTransformToBlobChanged(EntityUid uid, BlobCarrierComponent component, TransformToBlobActionEvent args)
    {
        TransformToBlob(uid, component);
    }

    private void OnStartup(EntityUid uid, BlobCarrierComponent component, ComponentStartup args)
    {
<<<<<<< HEAD
        _action.AddAction(uid, ref component.TransformToBlob ,ActionTransformToBlob);
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703))
        _action.AddAction(uid, ref component.TransformToBlob, ActionTransformToBlob);
=======
        _action.AddAction(uid, ref component.TransformToBlob, ActionTransformToBlob);
        EnsureComp<BlobSpeakComponent>(uid).OverrideName = false;

        if (HasComp<ActorComponent>(uid))
            return;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703))

        var ghostRole = EnsureComp<GhostRoleComponent>(uid);
        EnsureComp<GhostTakeoverAvailableComponent>(uid);
        ghostRole.RoleName = Loc.GetString("blob-carrier-role-name");
        ghostRole.RoleDescription = Loc.GetString("blob-carrier-role-desc");
        ghostRole.RoleRules = Loc.GetString("blob-carrier-role-rules");
<<<<<<< HEAD

        EnsureComp<BlobSpeakComponent>(uid);
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703))

        EnsureComp<BlobSpeakComponent>(uid).OverrideName = false;
=======
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703))
    }

    private void OnShutdown(EntityUid uid, BlobCarrierComponent component, ComponentShutdown args)
    {

    }

    private void OnMobStateChanged(EntityUid uid, BlobCarrierComponent component, MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
        {
            TransformToBlob(uid, component);
        }
    }

    private void TransformToBlob(EntityUid uid, BlobCarrierComponent carrier)
    {
        var xform = Transform(uid);
        if (!_mapManager.TryGetGrid(xform.GridUid, out var map))
            return;

        if (_mind.TryGetMind(uid, out var mindId, out var mind) && mind.UserId != null)
        {
<<<<<<< HEAD
            var core = Spawn(carrier.CoreBlobPrototype, xform.Coordinates);
||||||| parent of b4570616f0 ([Fix] Very hot blob fix (#792))
            var core = Spawn(ent.Comp.CoreBlobPrototype, xform.Coordinates);
=======
            var core = Spawn(ent.Comp.CoreBlobPrototype, xform.Coordinates);
            var ghostRoleComp = EnsureComp<GhostRoleComponent>(core);

            // Unfortunately we have to manually turn this off so we don't need to make more prototypes.
            _ghost.UnregisterGhostRole((core, ghostRoleComp));
>>>>>>> b4570616f0 ([Fix] Very hot blob fix (#792))

            if (!TryComp<BlobCoreComponent>(core, out var blobCoreComponent))
                return;

            _blobCoreSystem.CreateBlobObserver(core, mind.UserId.Value, blobCoreComponent);
        }
        else
        {
<<<<<<< HEAD
            Spawn(carrier.CoreBlobGhostRolePrototype, xform.Coordinates);
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703))
            Spawn(ent.Comp.CoreBlobGhostRolePrototype, xform.Coordinates);
=======
            Spawn(ent.Comp.CoreBlobPrototype, xform.Coordinates);
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703))
        }

        _bodySystem.GibBody(uid);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var blobFactoryQuery = EntityQueryEnumerator<BlobCarrierComponent>();
        while (blobFactoryQuery.MoveNext(out var ent, out var comp))
        {
            if (!comp.HasMind)
                return;

            comp.TransformationTimer += frameTime;

            if (_gameTiming.CurTime < comp.NextAlert)
                continue;

            var remainingTime = Math.Round(comp.TransformationDelay - comp.TransformationTimer, 0);
            _popup.PopupEntity(Loc.GetString("carrier-blob-alert", ("second", remainingTime)), ent, ent, PopupType.LargeCaution);

            comp.NextAlert = _gameTiming.CurTime + TimeSpan.FromSeconds(comp.AlertInterval);

            if (!(comp.TransformationTimer >= comp.TransformationDelay))
                continue;

            TransformToBlob(ent, comp);
        }
    }
}
