using System.Linq;
using System.Numerics;
using Content.Server.Administration;
using Content.Server.Construction.Conditions;
using Content.Server.Popups;
using Content.Server.Shuttles.Systems;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Crescent.Radar;
using Content.Shared.Interaction;
using Content.Shared.Interaction.Events;
using Content.Shared.PointCannons;
using Content.Shared.Popups;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.UserInterface;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Shuttles.Components;
using Robust.Server.GameObjects;
using Robust.Server.GameStates;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Player;
using System;
using Content.Server._Crescent.Hardpoint;
using Content.Server.Power.Components;
using Content.Shared._Crescent.Hardpoints;
using Content.Shared.Communications;
using Content.Shared.Physics;

namespace Content.Server.PointCannons;

public class PointCannonSystem : EntitySystem
{
    [Dependency] private readonly ISharedPlayerManager _playerMan = default!;
    [Dependency] private readonly TransformSystem _formSys = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _uiSys = default!;
    [Dependency] private readonly PopupSystem _popSys = default!;
    [Dependency] private readonly QuickDialogSystem _dialogSys = default!;
    [Dependency] private readonly GunSystem _gunSys = default!;
    [Dependency] private readonly ShuttleConsoleSystem _shuttleConSys = default!;
    [Dependency] private readonly PvsOverrideSystem _pvsSys = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly MapSystem _maps = default!;
    [Dependency] private readonly HardpointSystem _hardpoint = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TargetingConsoleComponent, ActivatableUIOpenAttemptEvent>(OnConsoleOpenAttempt);
        SubscribeLocalEvent<TargetingConsoleComponent, BoundUserInterfaceMessageAttempt>(BUIValidation);
        SubscribeLocalEvent<TargetingConsoleComponent, BoundUIOpenedEvent>(OnConsoleOpened);
        SubscribeLocalEvent<TargetingConsoleComponent, BoundUIClosedEvent>(OnConsoleClosed);
        SubscribeLocalEvent<TargetingConsoleComponent, TargetingConsoleFireMessage>(OnConsoleFire);
        SubscribeLocalEvent<TargetingConsoleComponent, TargetingConsoleGroupChangedMessage>(OnConsoleGroupChanged);
        SubscribeLocalEvent<TargetingConsoleComponent, ComponentRemove>(OnConsoleDelete);
        SubscribeLocalEvent<TargetingConsoleComponent, AnchorStateChangedEvent>(OnConsoleAnchor);

        SubscribeLocalEvent<PointCannonComponent, EntityTerminatingEvent>(OnCannonDetach);
        SubscribeLocalEvent<PointCannonComponent, EntParentChangedMessage>(OnCannonDetach);
        SubscribeLocalEvent<PointCannonComponent, ReAnchorEvent>(OnCannonDetach);

        SubscribeLocalEvent<MapGridComponent, GridFixtureChangeEvent>(OnGridShapeChange);

        SubscribeLocalEvent<PointCannonLinkToolComponent, UseInHandEvent>(OnLinkToolHandUse);
        SubscribeLocalEvent<PointCannonComponent, InteractUsingEvent>(OnLinkToolUse);
    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TargetingConsoleComponent>();
        while (query.MoveNext(out var uid, out var console))
        {
            if (!_uiSys.IsUiOpen(uid, TargetingConsoleUiKey.Key))
                continue;
            UpdateConsoleState(uid, console);
        }
    }

    private void OnGridShapeChange(EntityUid gridUid, MapGridComponent grid, ref GridFixtureChangeEvent args)
    {
        //Logger.Error($"Running grid fixture change on {MetaData(gridUid).EntityName}, NUMBER {gridUid}");
        HashSet<Entity<TargetingConsoleComponent>> targetingConsoles = new();
        _lookup.GetGridEntities(gridUid, targetingConsoles);
        foreach (var console in targetingConsoles)
        {
            UnlinkAllCannonsFromConsole(console.Owner, console.Comp);
            LinkAllCannonsToConsole(console.Owner, console.Comp);
        }
        _hardpoint.QueueHardpointRefresh(gridUid);
    }

    private void UnlinkAllCannonsFromConsole(EntityUid console, TargetingConsoleComponent comp)
    {
        // we need to create a copy of the dictionary else we modify the enumerable we act on
        foreach (var (group, cannons) in new Dictionary<string, List<EntityUid>>(comp.CannonGroups))
        {
            foreach (var cannon in new List<EntityUid>(cannons))
            {
                UnlinkConsole(cannon, console, comp);
            }
        }
    }
    private void OnConsoleDelete<T>(EntityUid console, TargetingConsoleComponent comp, ref T args)
    {
        UnlinkAllCannonsFromConsole(console, comp);
    }

    private void OnConsoleAnchor(EntityUid console, TargetingConsoleComponent comp, ref AnchorStateChangedEvent args)
    {
        if (!args.Anchored)
        {
            OnConsoleDelete(console, comp, ref args);
            return;
        }

        LinkAllCannonsToConsole(console, comp);
    }

    public void LinkAllCannonsToConsole(EntityUid console, TargetingConsoleComponent comp)
    {
        var gridUid = Transform(console).GridUid;
        if (gridUid is null)
            return;
        HashSet<Entity<PointCannonComponent>> cannonList = new();
        _lookup.GetGridEntities(gridUid.Value, cannonList);
        foreach (var cannon in cannonList)
        {
            if (Transform(cannon.Owner).Anchored == false)
                continue;
            LinkCannon(cannon.Owner, console, comp, MetaData(cannon.Owner).EntityName);
        }
    }

    public void LinkCannonToAllConsoles(EntityUid cannon)
    {
        var gridUid = Transform(cannon).GridUid;
        if (gridUid is null)
            return;
        HashSet<Entity<TargetingConsoleComponent>> consoleList = new();
        _lookup.GetGridEntities(gridUid.Value, consoleList);
        foreach (var console in consoleList)
        {
            if (Transform(console.Owner).Anchored == false)
                continue;
            LinkCannon(cannon, console.Owner, console.Comp, MetaData(cannon).EntityName);
        }
    }

    private void OnConsoleOpenAttempt(EntityUid uid, TargetingConsoleComponent component, ActivatableUIOpenAttemptEvent args)
    {
        var uis = _uiSys.GetActorUis(args.User);

        var ourGridUid = Transform(uid).GridUid;
        if (ourGridUid is null)
        {
            args.Cancel();
            return;
        }

        foreach (var (_, key) in uis)
        {
            if (key is ShuttleConsoleUiKey.Key)
            {
                args.Cancel();
                _popSys.PopupEntity(Loc.GetString("targeting-rejection-shuttle-console"), args.User, args.User, PopupType.LargeCaution);
            }
        }
    }

    private void BUIValidation(EntityUid uid, TargetingConsoleComponent component, BoundUserInterfaceMessageAttempt args)
    {
        var uis = _uiSys.GetActorUis(args.Actor);

        foreach (var (_, key) in uis)
        {
            if (key is ShuttleConsoleUiKey.Key)
            {
                args.Cancel();
            }
        }
    }
    private void OnConsoleOpened(Entity<TargetingConsoleComponent> uid, ref BoundUIOpenedEvent args)
    {
        uid.Comp.RegenerateCannons = true;

        if (_playerMan.TryGetSessionByEntity(args.Actor, out var session))
            TogglePvsOverride(uid.Comp.CurrentGroup, [session], true);
    }

    private void OnConsoleClosed(Entity<TargetingConsoleComponent> uid, ref BoundUIClosedEvent args)
    {
        if (_playerMan.TryGetSessionByEntity(args.Actor, out var session))
            TogglePvsOverride(uid.Comp.CurrentGroup, [session], false);
    }

    private void OnCannonDetach<T>(Entity<PointCannonComponent> uid,ref T args)
    {
        UnlinkCannon(uid);
    }



    private void OnLinkToolUse(Entity<PointCannonComponent> uid, ref InteractUsingEvent args)
    {
        if (!TryComp<PointCannonLinkToolComponent>(args.Used, out var linkTool))
            return;

        EntityUid? gridUid = Transform(uid).GridUid;
        if (gridUid == null)
            return;

        var query = EntityManager.AllEntityQueryEnumerator<TransformComponent, TargetingConsoleComponent>();
        while (query.MoveNext(out var consoleUid, out var form, out var console))
        {
            if (form.GridUid == gridUid)
                LinkCannon(uid, consoleUid, console, linkTool.GroupName);
        }

        _popSys.PopupEntity($"Added to {linkTool.GroupName}", args.User);
    }

    private void OnLinkToolHandUse(Entity<PointCannonLinkToolComponent> uid, ref UseInHandEvent args)
    {
        if (!_playerMan.TryGetSessionByEntity(args.User, out var session))
            return;

        _dialogSys.OpenDialog(session, "Group name", "Name (case insensitive)", (string name) =>
        {
            uid.Comp.GroupName = name == "" ? "all" : name.ToLower();
        });
    }

    public void LinkCannon(EntityUid cannonUid, EntityUid consoleUid, TargetingConsoleComponent console, string group)
    {
        if(!TryComp<PointCannonComponent>(cannonUid, out var cannonComponent))
            return;
        if (!console.CannonGroups.ContainsKey(group))
            console.CannonGroups[group] = [];

        if (console.CannonGroups[group].Contains(cannonUid))
        {
            // SPCR 2024 - For fixing the old ships before we added this functionality
            if (cannonComponent.LinkedConsoleId is null)
                cannonComponent.LinkedConsoleId = consoleUid;
            return;
        }

        console.CannonGroups[group].Add(cannonUid);
        if (group != "all" && !console.CannonGroups["all"].Contains(cannonUid))
            console.CannonGroups["all"].Add(cannonUid);

        console.RegenerateCannons = true;
        cannonComponent.LinkedConsoleId = consoleUid;
        cannonComponent.LinkedConsoleIds.Add(consoleUid);


        if (group == console.CurrentGroupName)
            TogglePvsOverride([cannonUid], GetUiSessions(consoleUid, TargetingConsoleUiKey.Key), true);
    }

    public void UnlinkCannon(EntityUid cannonUid)
    {
        if (!TryComp<PointCannonComponent>(cannonUid, out var cannonComp))
            return;
        foreach (var consoleUid in cannonComp.LinkedConsoleIds)
        {
            var console = Comp<TargetingConsoleComponent>(consoleUid);
            foreach (string group in console.CannonGroups.Keys.ToList())
            {
                console.CannonGroups[group].Remove(cannonUid);
                if (console.CannonGroups[group].Count == 0 && group != "all")
                {
                    console.CannonGroups.Remove(group);
                    if (console.CurrentGroupName == group)
                        console.CurrentGroupName = "all";
                }
            }

            console.RegenerateCannons = true;

            TogglePvsOverride([cannonUid], GetUiSessions(consoleUid, TargetingConsoleUiKey.Key), false);
        }

        cannonComp.LinkedConsoleIds.Clear();
    }

    public void UnlinkConsole(EntityUid cannonUid, EntityUid consoleUid, TargetingConsoleComponent comp)
    {
        if (!TryComp<PointCannonComponent>(cannonUid, out var cannonComp))
            return;
        cannonComp.LinkedConsoleIds.Remove(consoleUid);
        var console = Comp<TargetingConsoleComponent>(consoleUid);
        foreach (string group in console.CannonGroups.Keys.ToList())
        {
            console.CannonGroups[group].Remove(cannonUid);
            if (console.CannonGroups[group].Count == 0  && group != "all")
            {
                console.CannonGroups.Remove(group);
                if (console.CurrentGroupName == group)
                    console.CurrentGroupName = "all";
            }
        }

        console.RegenerateCannons = true;

        TogglePvsOverride([cannonUid], GetUiSessions(consoleUid, TargetingConsoleUiKey.Key), false);
    }

    public void UpdateConsoleState(EntityUid uid, TargetingConsoleComponent console)
    {
        NavInterfaceState navState = _shuttleConSys.GetNavState(uid, _shuttleConSys.GetAllDocks());
        IFFInterfaceState iffState = _shuttleConSys.GetIFFState(uid, console.RegenerateCannons ? null : console.PrevState?.IFFState.Turrets);

        TargetingConsoleBoundUserInterfaceState consoleState = new(
            navState,
            iffState,
            console.RegenerateCannons ? console.CannonGroups.Keys.ToList() : null,
            GetNetEntityList(console.CurrentGroup));

        console.RegenerateCannons = false;
        console.PrevState = consoleState;
        _uiSys.SetUiState(uid, TargetingConsoleUiKey.Key, consoleState);
    }

    private void OnConsoleFire(EntityUid uid, TargetingConsoleComponent console, TargetingConsoleFireMessage ev)
    {
        for (int i = 0; i < console.CurrentGroup.Count;)
        {
            EntityUid cannonUid = console.CurrentGroup[i];
            if (Deleted(cannonUid))
            {
                console.CurrentGroup.RemoveAt(i);
                continue;
            }

            TryFireCannon(cannonUid, ev.Coordinates);
            i++;
        }
    }

    private void OnConsoleGroupChanged(Entity<TargetingConsoleComponent> uid, ref TargetingConsoleGroupChangedMessage args)
    {
        string prevGroup = uid.Comp.CurrentGroupName;
        uid.Comp.CurrentGroupName = args.GroupName;
        uid.Comp.RegenerateCannons = true;

        List<ICommonSession> sessions = GetUiSessions(uid, TargetingConsoleUiKey.Key);
        TogglePvsOverride(uid.Comp.CannonGroups[prevGroup], sessions, false);
        TogglePvsOverride(uid.Comp.CurrentGroup, sessions, true);
    }

    private bool TryFireCannon(
        EntityUid uid,
        Vector2 pos,
        TransformComponent? form = null,
        GunComponent? gun = null,
        PointCannonComponent? cannon = null)
    {
        if (!Resolve(uid, ref form) || !Resolve(uid, ref gun) || !Resolve(uid, ref cannon))
            return false;

        if (form.MapUid == null || !_gunSys.CanShoot(gun))
            return false;
        if (!TryComp<HardpointAnchorableOnlyComponent>(uid, out var anchorComp))
            return false;
        if (anchorComp.anchoredTo is null)
            return false;
        if (!TryComp<ApcPowerReceiverComponent>(anchorComp.anchoredTo, out var powerComp))
            return false;
        if (!powerComp.Powered)
            return false;
        EntityCoordinates entPos = new EntityCoordinates(uid, new Vector2(0, -1));
        if (!TryComp<HardpointFixedMountComponent>(anchorComp.anchoredTo, out var fixedComp))
        {
            Vector2 cannonPos = _formSys.GetWorldPosition(form);
            _formSys.SetWorldRotation(uid, Angle.FromWorldVec(pos - cannonPos));
            entPos = new(form.MapUid.Value, pos);
        }

        if (!SafetyCheck(form.LocalRotation - Math.PI / 2, cannon))
            return false;
        _gunSys.AttemptShoot(uid, uid, gun, entPos);
        return true;
    }

    public bool SafetyCheck(Angle ang, PointCannonComponent cannon)
    {
        foreach ((Angle start, Angle width) in cannon.ObstructedRanges)
        {
            if (CrescentHelpers.AngInSector(ang, start, width))
                return false;
        }

        return true;
    }

    public void RefreshFiringRanges(EntityUid uid, TransformComponent? form = null, GunComponent? gun = null, PointCannonComponent? cannon = null, int? range = null)
    {
        if (!Resolve(uid, ref form) || !Resolve(uid, ref gun) || !Resolve(uid, ref cannon))
            return;
        // 10 meters the default if one is not provided
        range ??= 10;

        cannon.ObstructedRanges = CalculateFiringRanges(uid, form, gun, cannon, range.Value);
        Dirty(uid, cannon);
    }

    private List<(Angle, Angle)> CalculateFiringRanges(EntityUid uid, TransformComponent form, GunComponent gun, PointCannonComponent cannon, int range)
    {
        if (form.GridUid == null)
            return new();

        List<(Angle, Angle)> ranges = new();
        HashSet<EntityUid> entities = _lookup.GetEntitiesInRange(uid, (float) range, LookupFlags.Static);
        foreach(var childUid in entities)
        {
            //checking if obstacle is not too far/close to the cannon
            TransformComponent otherForm = Transform(childUid);
            // dont care about other grids
            if (otherForm.GridUid != form.GridUid)
                continue;
            Vector2 dir = otherForm.LocalPosition - form.LocalPosition;

            //checking that obstacle is anchored and solid
            if (!otherForm.Anchored || !TryComp<PhysicsComponent>(childUid, out var body) ||  !body.Hard || (body.CollisionLayer & (int)CollisionGroup.BulletImpassable) == 0)
                continue;

            //calculating circular sector that obstacle occupies relative to the cannon
            (Angle start0, Angle width0) = GetObstacleSector(dir);
            ranges.Add((start0, width0));

            (Angle start2, Angle width2) = (start0, width0);

            //checking whether new sector overlaps with any existing ones and combining them if so
            List<(Angle, Angle)> overlaps = new();
            foreach ((Angle start1, Angle width1) in ranges)
            {
                if (CrescentHelpers.AngSectorsOverlap(start0, width0, start1, width1))
                {
                    (start2, width2) = CrescentHelpers.AngCombinedSector(start2, width2, start1, width1);
                    overlaps.Add((start1, width1));
                }
            }

            foreach ((Angle start1, Angle width1) in overlaps)
            {
                ranges.Remove((start1, width1));
            }
            ranges.Add((start2, width2));
        }

        Angle maxSpread = gun.MaxAngle + Angle.FromDegrees(10);
        Angle clearance = maxSpread + cannon.ClearanceAngle;

        for (int i = 0; i < ranges.Count; i++)
        {
            ranges[i] = (CrescentHelpers.AngNormal(ranges[i].Item1 - clearance / 2), ranges[i].Item2 + clearance);
        }

        return ranges;
    }

    //delta between cannon's and obstacle's position
    private (Angle, Angle) GetObstacleSector(Vector2 delta)
    {
        Angle dirAngle = CrescentHelpers.AngNormal(new Angle(delta));
        Vector2 a, b;

        //this can be done without ugly conditional below, by rotating tile's square by delta's angle and finding left- and rightmost points,
        //but this certainly will be heavier and less clear
        if (dirAngle % (Math.PI * 0.5) == 0)
        {
            switch (dirAngle.Theta)
            {
                case 0:
                case Math.Tau:
                    a = delta - Vector2Helpers.Half;
                    b = new Vector2(delta.X - 0.5f, delta.Y + 0.5f);
                    break;
                case Math.PI * 0.5:
                    a = delta - Vector2Helpers.Half;
                    b = new Vector2(delta.X + 0.5f, delta.Y - 0.5f);
                    break;
                case Math.PI:
                    a = delta + Vector2Helpers.Half;
                    b = new Vector2(delta.X + 0.5f, delta.Y - 0.5f);
                    break;
                case Math.PI * 1.5:
                    a = delta + Vector2Helpers.Half;
                    b = new Vector2(delta.X - 0.5f, delta.Y + 0.5f);
                    break;
                default:
                    return (double.NaN, double.NaN);
            }
        }
        else if (dirAngle > 0 && dirAngle < Math.PI * 0.5 || dirAngle > Math.PI && dirAngle < Math.PI * 1.5)
        {
            a = new Vector2(delta.X - 0.5f, delta.Y + 0.5f);
            b = new Vector2(delta.X + 0.5f, delta.Y - 0.5f);
        }
        else
        {
            a = delta + Vector2Helpers.Half;
            b = delta - Vector2Helpers.Half;
        }

        Angle start = CrescentHelpers.AngNormal(new Angle(a));
        Angle end = CrescentHelpers.AngNormal(new Angle(b));
        Angle width = Angle.ShortestDistance(start, end);
        if (width < 0)
        {
            start = end;
            width = -width;
        }

        return (start, width);
    }

    private List<ICommonSession> GetUiSessions(EntityUid uid, Enum key)
    {
        List<ICommonSession> sessions = new();
        foreach (EntityUid actorUid in _uiSys.GetActors(uid, TargetingConsoleUiKey.Key))
        {
            if (_playerMan.TryGetSessionByEntity(actorUid, out var session))
                sessions.Add(session);
        }
        return sessions;
    }

    private void TogglePvsOverride(IEnumerable<EntityUid> uids, IEnumerable<ICommonSession> sessions, bool enable)
    {
        foreach (ICommonSession session in sessions)
        {
            foreach (EntityUid uid in uids)
            {
                if (!Exists(uid))
                    continue;

                if (enable)
                {
                    _pvsSys.AddSessionOverride(uid, session);
                }
                else
                {
                    _pvsSys.RemoveSessionOverride(uid, session);
                }
            }
        }
    }
}
