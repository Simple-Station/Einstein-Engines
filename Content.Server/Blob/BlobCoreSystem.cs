using System.Linq;
using System.Numerics;
<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
using System.Threading;
using Content.Server.Actions;
=======
using System.Threading;
using System.Threading.Tasks;
using Content.Server.Actions;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
using Content.Server.AlertLevel;
<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
using Content.Server.Chat.Managers;
||||||| parent of 54ef38c02d (Fix blob + specforce (#671)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
using Content.Server.Backmen.Blob.Components;
using Content.Server.Backmen.GameTicking.Rules.Components;
using Content.Server.Chat.Managers;
=======
using Content.Server.Backmen.Blob.Components;
using Content.Server.Backmen.GameTicking.Rules.Components;
using Content.Server.Backmen.Objectives;
<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
>>>>>>> 54ef38c02d (Fix blob + specforce (#671)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
using Content.Server.Explosion.Components;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
using Content.Server.Explosion.Components;
=======
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
using Content.Server.Explosion.EntitySystems;
using Content.Server.Fluids.EntitySystems;
using Content.Server.GameTicking;
<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.Objectives;
using Content.Server.Roles;
||||||| parent of 54ef38c02d (Fix blob + specforce (#671)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
using Content.Server.Mind;
using Content.Server.Objectives.Conditions;
=======
>>>>>>> 54ef38c02d (Fix blob + specforce (#671)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
using Content.Server.RoundEnd;
using Content.Server.Station.Systems;
<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
using Content.Server.Store.Components;
using Content.Server.Store.Systems;
=======
using Content.Server.Store.Systems;
using Content.Shared.Actions;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
using Content.Shared.Alert;
using Content.Shared.Blob;
using Content.Shared.Damage;
using Content.Shared.Destructible;
using Content.Shared.FixedPoint;
using Content.Shared.Popups;
using Content.Shared.Roles;
using Content.Shared.Weapons.Melee;
using Robust.Server.GameObjects;
using Robust.Shared.CPUJob.JobQueues;
using Robust.Shared.CPUJob.JobQueues.Queues;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Network;
<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
using Robust.Shared.Prototypes;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
=======
using Robust.Shared.Player;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs

namespace Content.Server.Blob;

public sealed class BlobCoreSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly AudioSystem _audioSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
=======
    [Dependency] private readonly SharedTransformSystem _transform = default!;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly BlobObserverSystem _blobObserver = default!;
    [Dependency] private readonly ExplosionSystem _explosionSystem = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly AlertLevelSystem _alertLevelSystem = default!;
    [Dependency] private readonly RoundEndSystem _roundEndSystem = default!;
<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
    [Dependency] private readonly MetaDataSystem _metaDataSystem = default!;
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly MapSystem _map = default!;
    [Dependency] private readonly StoreSystem _storeSystem = default!;
=======
    [Dependency] private readonly MetaDataSystem _metaDataSystem = default!;
    [Dependency] private readonly ActionsSystem _action = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly StoreSystem _storeSystem = default!;
    [Dependency] private readonly BlobTileSystem _blobTile = default!;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs

<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
    private EntityQuery<BlobTileComponent> _tile;
    private EntityQuery<BlobFactoryComponent> _factory;

    [ValidatePrototypeId<EntityPrototype>] private const string BlobCaptureObjective = "BlobCaptureObjective";
=======
    private EntityQuery<BlobTileComponent> _tile;
    private EntityQuery<BlobFactoryComponent> _factory;
    private EntityQuery<BlobNodeComponent> _node;

    [ValidatePrototypeId<AlertPrototype>]
    private const string BlobHealth = "BlobHealth";
    [ValidatePrototypeId<AlertPrototype>]
    private const string BlobResource = "BlobResource";
    [ValidatePrototypeId<CurrencyPrototype>]
    private const string BlobMoney = "BlobPoint";

    private readonly ReaderWriterLockSlim _pointsChange = new();

>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BlobCoreComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<BlobCoreComponent, DestructionEventArgs>(OnDestruction);
<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
        SubscribeLocalEvent<BlobCoreComponent, DamageChangedEvent>(OnDamaged);
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs

=======
        SubscribeLocalEvent<BlobCoreComponent, DamageChangedEvent>(OnDamaged);
        SubscribeLocalEvent<BlobCoreComponent, EntityTerminatingEvent>(OnTerminating);
        SubscribeLocalEvent<BlobCoreComponent, BlobTransformTileActionEvent>(OnTileTransform);

>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
        SubscribeLocalEvent<BlobCoreComponent, PlayerAttachedEvent>(OnPlayerAttached);
<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
||||||| parent of 54ef38c02d (Fix blob + specforce (#671)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
        SubscribeLocalEvent<BlobCoreComponent, EntityTerminatingEvent>(OnTerminating);


        SubscribeLocalEvent<BlobCaptureConditionComponent, ObjectiveGetProgressEvent>(OnBlobCaptureProgress);
        SubscribeLocalEvent<BlobCaptureConditionComponent, ObjectiveAfterAssignEvent>(OnBlobCaptureInfo);

        _tile = GetEntityQuery<BlobTileComponent>();
        _factory = GetEntityQuery<BlobFactoryComponent>();
=======
        SubscribeLocalEvent<BlobCoreComponent, EntityTerminatingEvent>(OnTerminating);


||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
        SubscribeLocalEvent<BlobCoreComponent, EntityTerminatingEvent>(OnTerminating);


=======
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
        SubscribeLocalEvent<BlobCaptureConditionComponent, ObjectiveGetProgressEvent>(OnBlobCaptureProgress);
        SubscribeLocalEvent<BlobCaptureConditionComponent, ObjectiveAfterAssignEvent>(OnBlobCaptureInfo);
        SubscribeLocalEvent<BlobCaptureConditionComponent, ObjectiveAssignedEvent>(OnBlobCaptureInfoAdd);


        _tile = GetEntityQuery<BlobTileComponent>();
        _factory = GetEntityQuery<BlobFactoryComponent>();
<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
>>>>>>> 54ef38c02d (Fix blob + specforce (#671)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
=======
        _node = GetEntityQuery<BlobNodeComponent>();
    }

    private const double KillCoreJobTime = 0.5;
    private readonly JobQueue _killCoreJobQueue = new(KillCoreJobTime);

    public sealed class KillBlobCore(
        BlobCoreSystem system,
        EntityUid? station,
        Entity<BlobCoreComponent> ent,
        double maxTime,
        CancellationToken cancellation = default)
        : Job<object>(maxTime, cancellation)
    {
        protected override async Task<object?> Process()
        {
            system.DestroyBlobCore(ent, station);
            return null;
        }
    }

    #region Events

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        _killCoreJobQueue.Process();
    }

    private void OnStartup(EntityUid uid, BlobCoreComponent component, ComponentStartup args)
    {
        if (!_tile.TryGetComponent(uid, out var blobTileComponent))
        {
            return;
        }

        if (!_node.TryGetComponent(uid, out var nodeComponent))
        {
            return;
        }

        ConnectBlobTile((uid, blobTileComponent), (uid, component), (uid, nodeComponent));

        var store = EnsureComp<StoreComponent>(uid);
        store.CurrencyWhitelist.Add(BlobMoney);

        UpdateAllAlerts((uid, component));
        ChangeChem(uid, component.DefaultChem, component);

        foreach (var action in component.ActionPrototypes)
        {
            EntityUid? actionUid = null;
            _action.AddAction(uid, ref actionUid, action);

            if (actionUid != null)
                component.Actions.Add(actionUid.Value);
        }
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
    }

<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
||||||| parent of 54ef38c02d (Fix blob + specforce (#671)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
    private void OnTerminating(EntityUid uid, BlobCoreComponent component, ref EntityTerminatingEvent args)
    {
        if (component.Observer != null && !TerminatingOrDeleted(component.Observer.Value) && !EntityManager.IsQueuedForDeletion(component.Observer.Value))
        {
            QueueDel(component.Observer.Value);
        }
    }

    #region Objective
    private void OnBlobCaptureInfo(EntityUid uid, BlobCaptureConditionComponent component, ref ObjectiveAfterAssignEvent args)
    {
        _metaDataSystem.SetEntityName(uid,Loc.GetString("objective-condition-blob-capture-title"));
        _metaDataSystem.SetEntityDescription(uid,Loc.GetString("objective-condition-blob-capture-description", ("count", component.Target)));
    }

    private void OnBlobCaptureProgress(EntityUid uid, BlobCaptureConditionComponent component, ref ObjectiveGetProgressEvent args)
    {
        // prevent divide-by-zero
        if (component.Target == 0)
        {
            args.Progress = 1;
            return;
        }

        if (args.Mind?.OwnedEntity == null)
        {
            args.Progress = 0;
            return;
        }

        if (!TryComp<BlobObserverComponent>(args.Mind.OwnedEntity, out var blobObserverComponent)
            || !TryComp<BlobCoreComponent>(blobObserverComponent.Core, out var blobCoreComponent))
        {
            args.Progress = 0;
            return;
        }
        args.Progress = (float) blobCoreComponent.BlobTiles.Count / (float) component.Target;
    }
    #endregion

=======
    private void OnTerminating(EntityUid uid, BlobCoreComponent component, ref EntityTerminatingEvent args)
    {
        CreateKillBlobCoreJob((uid, component));
    }

    private void OnDestruction(EntityUid uid, BlobCoreComponent component, DestructionEventArgs args)
    {
        CreateKillBlobCoreJob((uid, component));
    }

    private void OnPlayerAttached(EntityUid uid, BlobCoreComponent component, PlayerAttachedEvent args)
    {
        var xform = Transform(uid);
        if (!HasComp<MapGridComponent>(xform.GridUid))
            return;

        CreateBlobObserver(uid, args.Player.UserId, component);
    }

    private void OnDamaged(EntityUid uid, BlobCoreComponent component, DamageChangedEvent args)
    {
        UpdateAllAlerts((uid, component));
    }

    private void OnTileTransform(EntityUid uid, BlobCoreComponent blobCoreComponent, BlobTransformTileActionEvent args)
    {
        TransformSpecialTile((uid, blobCoreComponent), args);
    }

    #endregion

    #region Objective

    private void OnBlobCaptureInfoAdd(Entity<BlobCaptureConditionComponent> ent, ref ObjectiveAssignedEvent args)
    {
        if (args.Mind.OwnedEntity == null)
        {
            args.Cancelled = true;
            return;
        }
        if (!TryComp<BlobObserverComponent>(args.Mind.OwnedEntity, out var blobObserverComponent)
            || !HasComp<BlobCoreComponent>(blobObserverComponent.Core))
        {
            args.Cancelled = true;
            return;
        }

        var station = _stationSystem.GetOwningStation(blobObserverComponent.Core);
        if (station == null)
        {
            args.Cancelled = true;
            return;
        }

        ent.Comp.Target = CompOrNull<StationBlobConfigComponent>(station)?.StageTheEnd ?? StationBlobConfigComponent.DefaultStageEnd;
    }

    private void OnBlobCaptureInfo(EntityUid uid, BlobCaptureConditionComponent component, ref ObjectiveAfterAssignEvent args)
    {
        _metaDataSystem.SetEntityName(uid,Loc.GetString("objective-condition-blob-capture-title"));
        _metaDataSystem.SetEntityDescription(uid,Loc.GetString("objective-condition-blob-capture-description", ("count", component.Target)));
    }

    private void OnBlobCaptureProgress(EntityUid uid, BlobCaptureConditionComponent component, ref ObjectiveGetProgressEvent args)
    {
        if (!TryComp<BlobObserverComponent>(args.Mind.OwnedEntity, out var blobObserverComponent)
            || !TryComp<BlobCoreComponent>(blobObserverComponent.Core, out var blobCoreComponent))
        {
            args.Progress = 0;
            return;
        }

        var target = component.Target;
        args.Progress = 0;

        if (target != 0)
            args.Progress = MathF.Min((float) blobCoreComponent.BlobTiles.Count / target, 1f);
        else
            args.Progress = 1f;
    }
    #endregion

<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
>>>>>>> 54ef38c02d (Fix blob + specforce (#671)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
    private void OnPlayerAttached(EntityUid uid, BlobCoreComponent component, PlayerAttachedEvent args)
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
    private void OnPlayerAttached(EntityUid uid, BlobCoreComponent component, PlayerAttachedEvent args)
=======
    public void UpdateAllAlerts(Entity<BlobCoreComponent> core, StoreComponent? store = null)
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
    {
<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
        var xform = Transform(uid);
        if (!_mapManager.TryGetGrid(xform.GridUid, out var map))
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
        var xform = Transform(uid);
        if (!HasComp<MapGridComponent>(xform.GridUid))
=======
        if (!Resolve(core, ref store))
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
            return;

        var component = core.Comp;

        if (component.Observer == null)
            return;

        // This one for points
        var pt = store.Balance.GetValueOrDefault(BlobMoney);
        var pointsSeverity = (short) Math.Clamp(Math.Round(pt.Float() / 10f), 0, 51);
        _alerts.ShowAlert(component.Observer.Value, BlobResource, pointsSeverity);

        // And this one for health.
        if (!TryComp<DamageableComponent>(core.Owner, out var damageComp))
            return;

        var currentHealth = component.CoreBlobTotalHealth - damageComp.TotalDamage;
        var healthSeverity = (short) Math.Clamp(Math.Round(currentHealth.Float() / 20f), 0, 20);

        _alerts.ShowAlert(component.Observer.Value, BlobHealth, healthSeverity);
    }

    public bool CreateBlobObserver(EntityUid blobCoreUid, NetUserId userId, BlobCoreComponent? core = null)
    {
        if (!Resolve(blobCoreUid, ref core))
            return false;

        var blobRule = EntityQuery<BlobRuleComponent>().FirstOrDefault();

        if (blobRule == null)
        {
            _gameTicker.StartGameRule("Blob", out var ruleEntity);
            blobRule = Comp<BlobRuleComponent>(ruleEntity);
        }

        var observer = Spawn(core.ObserverBlobPrototype, xform.Coordinates);

        core.Observer = observer;

        if (!TryComp<BlobObserverComponent>(observer, out var blobObserverComponent))
            return false;

        blobObserverComponent.Core = blobCoreUid;

        _mindSystem.TryGetMind(userId, out var mind);
        if (mind == null)
            return false;

        _mindSystem.TransferTo(mind, observer, ghostCheckOverride: false);

        _alerts.ShowAlert(observer, AlertType.BlobHealth, (short) Math.Clamp(Math.Round(core.CoreBlobTotalHealth.Float() / 10f), 0, 20));

        var antagPrototype = _prototypeManager.Index<AntagPrototype>(core.AntagBlobPrototypeId);
        var blobRole = new BlobRole(mind, antagPrototype);

        _mindSystem.AddRole(mind, blobRole);
        SendBlobBriefing(mind);

        blobRule.Blobs.Add(blobRole);

        if (_prototypeManager.TryIndex<ObjectivePrototype>("BlobCaptureObjective", out var objective)
            && objective.CanBeAssigned(mind))
        {
            _mindSystem.TryAddObjective(blobRole.Mind, objective);
        }

        if (_mindSystem.TryGetSession(mind, out var session))
        {
            _audioSystem.PlayGlobal(core.GreetSoundNotification, session);
        }

        _blobObserver.UpdateUi(observer, blobObserverComponent);

        return true;
    }

<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
    private void SendBlobBriefing(Mind.Mind mind)
    {
        if (_mindSystem.TryGetSession(mind, out var session))
        {
            _chatManager.DispatchServerMessage(session, Loc.GetString("blob-role-greeting"));
        }
    }

    private void OnDamaged(EntityUid uid, BlobCoreComponent component, DamageChangedEvent args)
    {
        var maxHealth = component.CoreBlobTotalHealth;
        var currentHealth = maxHealth - args.Damageable.TotalDamage;

        if (component.Observer != null)
            _alerts.ShowAlert(component.Observer.Value, AlertType.BlobHealth, (short) Math.Clamp(Math.Round(currentHealth.Float() / 10f), 0, 20));
    }

    private void OnStartup(EntityUid uid, BlobCoreComponent component, ComponentStartup args)
    {
        ChangeBlobPoint(uid, 0, component);

        if (TryComp<BlobTileComponent>(uid, out var blobTileComponent))
        {
            blobTileComponent.Core = uid;
            blobTileComponent.Color = component.ChemСolors[component.CurrentChem];
            Dirty(blobTileComponent);
        }

        component.BlobTiles.Add(uid);

        ChangeChem(uid, component.DefaultChem, component);
    }

||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
    [ValidatePrototypeId<EntityPrototype>] private const string ActionHelpBlob = "ActionHelpBlob";
    [ValidatePrototypeId<EntityPrototype>] private const string ActionSwapBlobChem = "ActionSwapBlobChem";
    [ValidatePrototypeId<EntityPrototype>] private const string ActionTeleportBlobToCore = "ActionTeleportBlobToCore";
    [ValidatePrototypeId<EntityPrototype>] private const string ActionTeleportBlobToNode = "ActionTeleportBlobToNode";
    [ValidatePrototypeId<EntityPrototype>] private const string ActionCreateBlobFactory = "ActionCreateBlobFactory";
    [ValidatePrototypeId<EntityPrototype>] private const string ActionCreateBlobResource = "ActionCreateBlobResource";
    [ValidatePrototypeId<EntityPrototype>] private const string ActionCreateBlobNode = "ActionCreateBlobNode";
    [ValidatePrototypeId<EntityPrototype>] private const string ActionCreateBlobbernaut = "ActionCreateBlobbernaut";
    [ValidatePrototypeId<EntityPrototype>] private const string ActionSplitBlobCore = "ActionSplitBlobCore";
    [ValidatePrototypeId<EntityPrototype>] private const string ActionSwapBlobCore = "ActionSwapBlobCore";

    private void OnStartup(EntityUid uid, BlobCoreComponent component, ComponentStartup args)
    {
        var store = EnsureComp<StoreComponent>(uid);
        store.CurrencyWhitelist.Add(BlobMoney);

        ChangeBlobPoint(uid, 0, component);

        if (_tile.TryGetComponent(uid, out var blobTileComponent))
        {
            blobTileComponent.Core = uid;
            blobTileComponent.Color = component.ChemСolors[component.CurrentChem];
            Dirty(uid, blobTileComponent);
        }

        component.BlobTiles.Add(uid);

        ChangeChem(uid, component.DefaultChem, component);

        _action.AddAction(uid, ref component.ActionHelpBlob, ActionHelpBlob);
        _action.AddAction(uid, ref component.ActionSwapBlobChem, ActionSwapBlobChem);
        _action.AddAction(uid, ref component.ActionTeleportBlobToCore, ActionTeleportBlobToCore);
        _action.AddAction(uid, ref component.ActionTeleportBlobToNode, ActionTeleportBlobToNode);
        _action.AddAction(uid, ref component.ActionCreateBlobFactory, ActionCreateBlobFactory);
        _action.AddAction(uid, ref component.ActionCreateBlobResource, ActionCreateBlobResource);
        _action.AddAction(uid, ref component.ActionCreateBlobNode, ActionCreateBlobNode);
        _action.AddAction(uid, ref component.ActionCreateBlobbernaut, ActionCreateBlobbernaut);
        _action.AddAction(uid, ref component.ActionSplitBlobCore, ActionSplitBlobCore);
        _action.AddAction(uid, ref component.ActionSwapBlobCore, ActionSwapBlobCore);
    }

=======
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
    public void ChangeChem(EntityUid uid, BlobChemType newChem, BlobCoreComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (newChem == component.CurrentChem)
            return;

        component.CurrentChem = newChem;
        foreach (var blobTile in component.BlobTiles)
        {
            if (!TryComp<BlobTileComponent>(blobTile, out var blobTileComponent))
                continue;

            blobTileComponent.Color = component.ChemСolors[newChem];
            Dirty(blobTileComponent);

<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
            if (TryComp<BlobFactoryComponent>(blobTile, out var blobFactoryComponent))
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
            if (_factory.TryGetComponent(blobTile, out var blobFactoryComponent))
=======
            ChangeBlobEntChem(blobTile, newChem);

            if (!_factory.TryGetComponent(blobTile, out var blobFactoryComponent))
                continue;

            if (!TryComp<BlobbernautComponent>(blobFactoryComponent.Blobbernaut, out var blobbernautComponent))
                continue;

            blobbernautComponent.Color = component.ChemСolors[newChem];
            Dirty(blobFactoryComponent.Blobbernaut.Value, blobbernautComponent);

            if (TryComp<MeleeWeaponComponent>(blobFactoryComponent.Blobbernaut, out var meleeWeaponComponent))
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
            {
                var blobbernautDamage = new DamageSpecifier();
                foreach (var keyValuePair in component.ChemDamageDict[component.CurrentChem].DamageDict)
                {
<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
                    blobbernautComponent.Color = component.ChemСolors[newChem];
<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
                    Dirty(blobbernautComponent);
||||||| parent of b1f1be5a79 (Cleanup (#593)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
                    Dirty(blobTile, blobbernautComponent);
=======
                    Dirty(blobFactoryComponent.Blobbernaut.Value, blobbernautComponent);
>>>>>>> b1f1be5a79 (Cleanup (#593)):Content.Server/Backmen/Blob/BlobCoreSystem.cs

                    if (TryComp<MeleeWeaponComponent>(blobFactoryComponent.Blobbernaut, out var meleeWeaponComponent))
                    {
                        var blobbernautDamage = new DamageSpecifier();
                        foreach (var keyValuePair in component.ChemDamageDict[component.CurrentChem].DamageDict)
                        {
                            blobbernautDamage.DamageDict.Add(keyValuePair.Key, keyValuePair.Value * 0.8f);
                        }
                        meleeWeaponComponent.Damage = blobbernautDamage;
                    }

                    ChangeBlobEntChem(blobFactoryComponent.Blobbernaut.Value, oldChem, newChem);
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
                    blobbernautComponent.Color = component.ChemСolors[newChem];
                    Dirty(blobFactoryComponent.Blobbernaut.Value, blobbernautComponent);

                    if (TryComp<MeleeWeaponComponent>(blobFactoryComponent.Blobbernaut, out var meleeWeaponComponent))
                    {
                        var blobbernautDamage = new DamageSpecifier();
                        foreach (var keyValuePair in component.ChemDamageDict[component.CurrentChem].DamageDict)
                        {
                            blobbernautDamage.DamageDict.Add(keyValuePair.Key, keyValuePair.Value * 0.8f);
                        }
                        meleeWeaponComponent.Damage = blobbernautDamage;
                    }

                    ChangeBlobEntChem(blobFactoryComponent.Blobbernaut.Value, oldChem, newChem);
=======
                    blobbernautDamage.DamageDict.Add(keyValuePair.Key, keyValuePair.Value * 0.8f);
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
                }
<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs

                foreach (var compBlobPod in blobFactoryComponent.BlobPods)
                {
                    if (TryComp<SmokeOnTriggerComponent>(compBlobPod, out var smokeOnTriggerComponent))
                    {
                        smokeOnTriggerComponent.SmokeColor = component.ChemСolors[newChem];
                    }
                }
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
/*
                foreach (var compBlobPod in blobFactoryComponent.BlobPods)
                {
                    if (TryComp<SmokeOnTriggerComponent>(compBlobPod, out var smokeOnTriggerComponent))
                    {
                        smokeOnTriggerComponent.SmokeColor = component.ChemСolors[newChem];
                    }
                }
                */
=======
                meleeWeaponComponent.Damage = blobbernautDamage;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
            }

            ChangeBlobEntChem(blobFactoryComponent.Blobbernaut.Value, newChem);
        }
    }

    private void ChangeBlobEntChem(EntityUid uid, BlobChemType newChem)
    {
        switch (newChem)
        {
            case BlobChemType.ExplosiveLattice:
                _damageable.SetDamageModifierSetId(uid, "ExplosiveLatticeBlob");
                _explosionSystem.SetExplosionResistance(uid, 0f, EnsureComp<ExplosionResistanceComponent>(uid));
                break;
            case BlobChemType.ElectromagneticWeb:
                _damageable.SetDamageModifierSetId(uid, "ElectromagneticWebBlob");
                break;
            default:
                _damageable.SetDamageModifierSetId(uid, "BaseBlob");
                break;
        }
    }

    /// <summary>
    /// Transforms one blob tile in another type or creates a new one from scratch.
    /// </summary>
    /// <param name="oldTileUid">Uid of the ols tile that's going to get deleted.</param>
    /// <param name="blobCore">Blob core that preformed the transformation. Make sure it isn't came from the BlobTileComponent of the target!</param>
    /// <param name="nearNode">Node will be used in ConnectBlobTile method.</param>
    /// <param name="newBlobTile">Type of a new blob tile.</param>
    /// <param name="coordinates">Coordinates of a new tile.</param>
    /// <seealso cref="ConnectBlobTile"/>
    /// <seealso cref="BlobCoreComponent"/>
    public bool TransformBlobTile(
        Entity<BlobTileComponent>? oldTileUid,
        Entity<BlobCoreComponent> blobCore,
        Entity<BlobNodeComponent>? nearNode,
        BlobTileType newBlobTile,
        EntityCoordinates coordinates)
    {
        if (oldTileUid != null)
        {
            if (oldTileUid.Value.Comp.Core != blobCore)
                return false;

            RemoveBlobTile(oldTileUid.Value, blobCore);
        }

        var blobCoreComp = blobCore.Comp;
        var blobTileUid = EntityManager.SpawnEntity(blobCoreComp.TilePrototypes[newBlobTile], coordinates);

        if (!_tile.TryGetComponent(blobTileUid, out var blobTileComp))
        {
            // Blob somehow spawned not a blob tile?
            return false;
        }

        ConnectBlobTile((blobTileUid, blobTileComp), blobCore, nearNode);
        ChangeBlobEntChem(blobTileUid, blobCoreComp.CurrentChem);

        Dirty(blobTileUid, blobTileComp);

        return true;
    }

    /// <summary>
    /// Adds BlobTile to blob core and node, if specified.
    /// </summary>
    /// <param name="tile">Entity of the blob tile.</param>
    /// <param name="core">Entity of the blob core.</param>
    /// <param name="node">If not null, tries to connect tile to the node by checking if their BlobTileType is presented in dictionary.</param>
    public void ConnectBlobTile(
        Entity<BlobTileComponent> tile,
        Entity<BlobCoreComponent> core,
        Entity<BlobNodeComponent>? node)
    {
        var coreComp = core.Comp;
        var tileComp = tile.Comp;

        coreComp.BlobTiles.Add(tile);

        tileComp.Color = coreComp.ChemСolors[coreComp.CurrentChem];
        tileComp.Core = core;
        Dirty(tile, tileComp);

        if (node == null)
            return;

        switch (tile.Comp.BlobTileType)
        {
            case BlobTileType.Factory:
                node.Value.Comp.BlobFactory = tile;
                Dirty(node.Value);
                break;
            case BlobTileType.Resource:
                node.Value.Comp.BlobResource = tile;
                Dirty(node.Value);
                break;
        }
    }

    public bool TryGetTargetBlobTile(WorldTargetActionEvent args, out Entity<BlobTileComponent>? blobTile)
    {
        blobTile = null;

        var gridUid = _transform.GetGrid(args.Target);

        if (!TryComp<MapGridComponent>(gridUid, out var gridComp))
        {
            return false;
        }

        Entity<MapGridComponent> grid = (gridUid.Value, gridComp);

        var centerTile = _mapSystem.GetLocalTilesIntersecting(grid,
                grid,
                new Box2(args.Target.Position, args.Target.Position))
            .ToArray();

        foreach (var tileRef in centerTile)
        {
            foreach (var ent in _mapSystem.GetAnchoredEntities(grid, grid, tileRef.GridIndices))
            {
                if (!_tile.TryGetComponent(ent, out var blobTileComponent))
                    continue;

                blobTile = (ent, blobTileComponent);
                return true;
            }
        }

        return false;
    }

    public bool CheckValidBlobTile(
        Entity<BlobTileComponent> tile,
        Entity<BlobNodeComponent>? node,
        bool requireNode,
        BlobTransformTileActionEvent args)
    {
        var coords = Transform(tile).Coordinates;

        var newTile = args.TileType;
        var checkTile = args.TransformFrom;
        var performer = args.Performer;

        if (tile.Comp.Core == null ||
            tile.Comp.BlobTileType == newTile ||
            tile.Comp.BlobTileType == BlobTileType.Core ||
            tile.Comp.BlobTileType != checkTile)
        {
            _popup.PopupCoordinates(Loc.GetString("blob-target-normal-blob-invalid"), coords, performer, PopupType.Large);
            return false;
        }

        var core = tile.Comp.Core.Value;

        if (checkTile == BlobTileType.Invalid)
            return true;

        // Handle node spawn
        if (newTile == BlobTileType.Node)
        {
            if (GetNearNode(coords, core.Comp.NodeRadiusLimit) == null)
                return true;

            _popup.PopupCoordinates(Loc.GetString("blob-target-close-to-node"), coords, performer, PopupType.Large);
            return false;
        }

        if (!requireNode)
            return true;

        if (node == null)
        {
            _popup.PopupCoordinates(Loc.GetString("blob-target-nearby-not-node"),
                coords,
                performer,
                PopupType.Large);
            return false;
        }

        if (_blobTile.IsEmptySpecial(node.Value, newTile))
            return true;

        _popup.PopupCoordinates(Loc.GetString("blob-target-already-connected"),
            coords,
            performer,
            PopupType.Large);
        return false;
    }

    public void TransformSpecialTile(Entity<BlobCoreComponent> blobCore, BlobTransformTileActionEvent args)
    {
        if (!TryGetTargetBlobTile(args, out var blobTile) || blobTile?.Comp.Core == null)
            return;

        var coords = Transform(blobTile.Value).Coordinates;
        var tileType = args.TileType;
        var nearNode = GetNearNode(coords);

        if (!CheckValidBlobTile(blobTile.Value, nearNode, args.RequireNode, args))
            return;

        if (!TryUseAbility(blobCore, blobCore.Comp.BlobTileCosts[tileType], coords))
            return;

        TransformBlobTile(
            blobTile,
            blobCore,
            nearNode,
            tileType,
            coords);
    }

    public void RemoveBlobTile(Entity<BlobTileComponent> tile, Entity<BlobCoreComponent> core)
    {
        QueueDel(tile);
        core.Comp.BlobTiles.Remove(tile);
    }

    private void DestroyBlobCore(Entity<BlobCoreComponent> core, EntityUid? stationUid)
    {
        var uid = core.Owner;
        var component = core.Comp;

        QueueDel(component.Observer);

        foreach (var blobTile in component.BlobTiles)
        {
            if (!TryComp<BlobTileComponent>(blobTile, out var blobTileComponent))
                continue;

            blobTileComponent.Core = null;
            blobTileComponent.Color = Color.White;
            Dirty(blobTileComponent);
        }

        var blobCoreQuery = EntityQueryEnumerator<BlobCoreComponent>();
        var isAllDie = 0;
        while (blobCoreQuery.MoveNext(out var ent, out _))
        {
            if (TerminatingOrDeleted(ent))
            {
                continue;
            }
            isAllDie++;
        }

        if (isAllDie <= 1)
        {
<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
            var blobFactoryQuery = EntityQueryEnumerator<BlobRuleComponent>();
            while (blobFactoryQuery.MoveNext(out var blobRuleUid, out var blobRuleComp))
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
            var blobFactoryQuery = EntityQueryEnumerator<BlobRuleComponent>();
            while (blobFactoryQuery.MoveNext(out _, out var blobRuleComp))
=======
            var blobRuleQuery = EntityQueryEnumerator<BlobRuleComponent>();
            while (blobRuleQuery.MoveNext(out _, out var blobRuleComp))
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
            {
                if (blobRuleComp.Stage == BlobStage.TheEnd ||
                    blobRuleComp.Stage == BlobStage.Default ||
                    stationUid == null)
                    continue;

                _alertLevelSystem.SetLevel(stationUid.Value, "green", true, true, true);
                _roundEndSystem.CancelRoundEndCountdown(null, false);
                blobRuleComp.Stage = BlobStage.Default;
            }
        }

        QueueDel(uid);
    }

    private void CreateKillBlobCoreJob(Entity<BlobCoreComponent> core)
    {
        var station = _stationSystem.GetOwningStation(core);
        var job = new KillBlobCore(this, station, core, KillCoreJobTime);
        _killCoreJobQueue.EnqueueJob(job);
    }

    public void RemoveTileWithReturnCost(Entity<BlobTileComponent> target, Entity<BlobCoreComponent> core)
    {
        RemoveBlobTile(target, core);

<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
        if (TryComp<BlobTileComponent>(tileBlob, out var blobTileComponent))
        {
            blobTileComponent.ReturnCost = returnCost;
            blobTileComponent.Core = coreTileUid;
            blobTileComponent.Color = blobCore.ChemСolors[blobCore.CurrentChem];
            Dirty(blobTileComponent);
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
        if (_tile.TryGetComponent(tileBlob, out var blobTileComponent))
        {
            blobTileComponent.ReturnCost = returnCost;
            blobTileComponent.Core = coreTileUid;
            blobTileComponent.Color = blobCore.ChemСolors[blobCore.CurrentChem];
            Dirty(tileBlob, blobTileComponent);
=======
        FixedPoint2 returnCost = 0;
        var tileComp = target.Comp;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs

        if (target.Comp.ReturnCost)
        {
            returnCost = core.Comp.BlobTileCosts[tileComp.BlobTileType];
        }

        if (returnCost <= 0)
            return;

        ChangeBlobPoint(core, returnCost);

        if (core.Comp.Observer == null)
            return;

        _popup.PopupCoordinates(Loc.GetString("blob-get-resource", ("point", returnCost)),
            Transform(target).Coordinates,
            core.Comp.Observer.Value,
            PopupType.LargeGreen);
    }

    public bool ChangeBlobPoint(Entity<BlobCoreComponent> core, FixedPoint2 amount, StoreComponent? store = null)
    {
        if (!Resolve(core, ref store))
            return false;

<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
        QueueDel(tileUid);
        blobCore.BlobTiles.Remove(tileUid);

        return true;
    }

    public bool ChangeBlobPoint(EntityUid uid, FixedPoint2 amount, BlobCoreComponent? component = null)
    {
        if (!Resolve(uid, ref component))
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
        QueueDel(tileUid);
        blobCore.BlobTiles.Remove(tileUid);

        return true;
    }

    [ValidatePrototypeId<AlertPrototype>]
    private const string BlobResource = "BlobResource";

    [ValidatePrototypeId<CurrencyPrototype>]
    private const string BlobMoney = "BlobPoint";

    private readonly ReaderWriterLockSlim _pointsChange = new();

    public bool ChangeBlobPoint(EntityUid uid, FixedPoint2 amount, BlobCoreComponent? component = null, StoreComponent? store = null)
    {
        if (!Resolve(uid, ref component) || !Resolve(uid, ref store))
=======
        if (!_pointsChange.TryEnterWriteLock(1000))
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
            return false;

<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
        component.Points += amount;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
        if (_pointsChange.TryEnterWriteLock(1000))
        {
            if (_storeSystem.TryAddCurrency(new Dictionary<string, FixedPoint2>
                    {
                        { BlobMoney, amount }
                    },
                    uid,
                    store))
            {
                var pt = store.Balance.GetValueOrDefault(BlobMoney);
=======
        if (_storeSystem.TryAddCurrency(new Dictionary<string, FixedPoint2>
                {
                    { BlobMoney, amount }
                },
                core,
                store))
        {
            UpdateAllAlerts(core);
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs

<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
        if (component.Observer != null)
            _alerts.ShowAlert(component.Observer.Value, AlertType.BlobResource, (short) Math.Clamp(Math.Round(component.Points.Float() / 10f), 0, 16));

        return true;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
                if (component.Observer != null)
                    _alerts.ShowAlert(component.Observer.Value, BlobResource, (short) Math.Clamp(Math.Round(pt.Float() / 10f), 0, 16));

                _pointsChange.ExitWriteLock();
                return true;
            }
            _pointsChange.ExitWriteLock();
            return false;
        }

        return false;
=======
            _pointsChange.ExitWriteLock();
            return true;
        }

        _pointsChange.ExitWriteLock();
        return false;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
    }

<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
    public bool TryUseAbility(EntityUid uid, EntityUid coreUid, BlobCoreComponent component, FixedPoint2 abilityCost)
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
    public bool TryUseAbility(EntityUid uid, EntityUid coreUid, BlobCoreComponent component, FixedPoint2 abilityCost, StoreComponent? store = null)
=======
    /// <summary>
    /// Writes off points for some blob core and creates popup on observer or specified coordinates.
    /// </summary>
    /// <param name="core">Blob core that is going to lose points.</param>
    /// <param name="abilityCost">Cost of the ability.</param>
    /// <param name="coordinates">If not null, coordinates for popup to appear.</param>
    /// <param name="store">StoreComponent</param>
    public bool TryUseAbility(Entity<BlobCoreComponent> core, FixedPoint2 abilityCost, EntityCoordinates? coordinates = null, StoreComponent? store = null)
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
    {
<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
        if (component.Points < abilityCost)
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
        if (!Resolve(coreUid, ref store))
            return false;

        var pt = store.Balance.GetValueOrDefault(BlobMoney);
        if (pt < abilityCost)
=======
        if (!Resolve(core, ref store))
            return false;

        var observer = core.Comp.Observer;
        var money = store.Balance.GetValueOrDefault(BlobMoney);

        if (observer == null)
            return false;

        if (money < abilityCost)
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
        {
            _popup.PopupEntity(Loc.GetString(
                "blob-not-enough-resources",
                ("point", abilityCost.Int() - money.Int())),
                observer.Value,
                PopupType.Large);
            return false;
        }

<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
        ChangeBlobPoint(coreUid, -abilityCost, component);

        return true;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
        return ChangeBlobPoint(coreUid, -abilityCost, component, store);
=======
        coordinates ??= Transform(observer.Value).Coordinates;

        _popup.PopupCoordinates(
            Loc.GetString("blob-spent-resource", ("point", abilityCost.Int())),
            coordinates.Value,
            observer.Value,
            PopupType.LargeCaution);

        ChangeBlobPoint(core, -abilityCost);
        return true;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
    }

<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
    public bool CheckNearNode(EntityUid observer, EntityCoordinates coords, MapGridComponent grid, BlobCoreComponent core)
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
    public bool CheckNearNode(EntityUid observer, EntityCoordinates coords, Entity<MapGridComponent> grid, BlobCoreComponent core)
=======
    /// <summary>
    /// Gets the nearest Blob node from some EntityCoordinates.
    /// </summary>
    /// <param name="coords">The EntityCoordinates to check from.</param>
    /// <param name="radius">Radius to check from coords.</param>
    /// <returns>Nearest blob node with it's component, null if wasn't founded.</returns>
    public Entity<BlobNodeComponent>? GetNearNode(
        EntityCoordinates coords,
        float radius = 3f)
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
    {
        var gridUid = _transform.GetGrid(coords)!.Value;

<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
        var innerTiles = grid.GetLocalTilesIntersecting(
            new Box2(coords.Position + new Vector2(-radius, -radius), coords.Position + new Vector2(radius, radius)), false).ToArray();
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
        var innerTiles = _map.GetLocalTilesIntersecting(grid,grid,
            new Box2(coords.Position + new Vector2(-radius, -radius), coords.Position + new Vector2(radius, radius)), false).ToArray();
=======
        if (!TryComp<MapGridComponent>(gridUid, out var grid))
            return null;

        var nearestDistance = float.MaxValue;
        var nodeComponent = new BlobNodeComponent();
        var nearestEntityUid = EntityUid.Invalid;

        var innerTiles = _mapSystem.GetLocalTilesIntersecting(
                gridUid,
                grid,
                new Box2(coords.Position + new Vector2(-radius, -radius),
                    coords.Position + new Vector2(radius, radius)),
                false)
            .ToArray();
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs

        foreach (var tileRef in innerTiles)
        {
<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
            foreach (var ent in grid.GetAnchoredEntities(tileRef.GridIndices))
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
            foreach (var ent in _map.GetAnchoredEntities(grid,grid,tileRef.GridIndices))
=======
            foreach (var ent in _mapSystem.GetAnchoredEntities(gridUid, grid, tileRef.GridIndices))
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
            {
<<<<<<< HEAD:Content.Server/Blob/BlobCoreSystem.cs
                if (HasComp<BlobNodeComponent>(ent) || HasComp<BlobCoreComponent>(ent))
                    return true;
||||||| parent of c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
                if (queryNode.HasComponent(ent) || queryCore.HasComponent(ent))
                    return true;
=======
                if (!_node.TryComp(ent, out var nodeComp))
                    continue;
                var tileCords = Transform(ent).Coordinates;
                var distance = Vector2.Distance(coords.Position, tileCords.Position);

                if (!(distance < nearestDistance))
                    continue;

                nearestDistance = distance;
                nearestEntityUid = ent;
                nodeComponent = nodeComp;
>>>>>>> c57c139059 ([Tweak] Blob Refactor Part 1: General Rewrite (#703)):Content.Server/Backmen/Blob/BlobCoreSystem.cs
            }
        }

        return nearestDistance > radius ? null : (nearestEntityUid, nodeComponent);
    }
}
