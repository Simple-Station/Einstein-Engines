/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared._CE.ZLevels.Core.EntitySystems;
using Content.Shared.Actions;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._CE.ZLevels.Core;

public sealed partial class CEZLevelsSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly ViewSubscriberSystem _viewSubscriber = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    private readonly EntProtoId _zEyeProto = "CEZLevelEye";

    private readonly TimeSpan _zLevelViewerUpdateRate = TimeSpan.FromSeconds(1f);
    private TimeSpan _nextZLevelViewerUpdate = TimeSpan.Zero;

    private void InitView()
    {
        SubscribeLocalEvent<PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<PlayerDetachedEvent>(OnPlayerDetached);

        SubscribeLocalEvent<CEZLevelViewerComponent, MapInitEvent>(OnViewerInit);
        SubscribeLocalEvent<CEZLevelViewerComponent, ComponentRemove>(OnCompRemove);

        SubscribeLocalEvent<CEZLevelViewerComponent, MapUidChangedEvent>(OnViewerMapUidChanged);
        SubscribeLocalEvent<CEZPhysicsComponent, CEZLevelFallMapEvent>(OnZLevelFall);
    }

    private void UpdateView(float frameTime)
    {
        if (_timing.CurTime < _nextZLevelViewerUpdate)
            return;
        _nextZLevelViewerUpdate = _timing.CurTime + _zLevelViewerUpdateRate;

        var query = EntityQueryEnumerator<CEZLevelViewerComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var viewer, out var xform))
        {
            foreach (var eye in viewer.Eyes)
            {
                _transform.SetWorldPosition(eye, _transform.GetWorldPosition(xform));
            }
        }
    }

    private void OnViewerInit(Entity<CEZLevelViewerComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent, ref ent.Comp.ZLevelActionEntity, ent.Comp.ActionProto);
        _meta.AddFlag(ent, MetaDataFlags.ExtraTransformEvents);
    }

    private void OnCompRemove(Entity<CEZLevelViewerComponent> ent, ref ComponentRemove args)
    {
        _actions.RemoveAction(ent.Comp.ZLevelActionEntity);
        _meta.RemoveFlag(ent, MetaDataFlags.ExtraTransformEvents);

        foreach (var eye in ent.Comp.Eyes)
        {
            QueueDel(eye);
        }
    }

    private void OnPlayerAttached(PlayerAttachedEvent ev)
    {
        var viewer = EnsureComp<CEZLevelViewerComponent>(ev.Entity);
        UpdateViewer((ev.Entity, viewer));
    }

    private void OnPlayerDetached(PlayerDetachedEvent ev)
    {
        RemComp<CEZLevelViewerComponent>(ev.Entity);
    }

    private void OnViewerMapUidChanged(Entity<CEZLevelViewerComponent> ent, ref MapUidChangedEvent args)
    {
        UpdateViewer(ent);
    }

    private void UpdateViewer(Entity<CEZLevelViewerComponent> ent)
    {
        var eyes = ent.Comp.Eyes;
        foreach (var eye in ent.Comp.Eyes)
        {
            QueueDel(eye);
        }
        eyes.Clear();

        if (!TryComp<ActorComponent>(ent, out var actor))
            return;

        var xform = Transform(ent);
        var map = xform.MapUid;

        if (map is null)
            return;

        var globalPos = _transform.GetWorldPosition(xform);

        for (var i = 1; i <= MaxZLevelsBelowRendering; i++)
        {
            if (!TryMapOffset(map.Value, -i, out var mapUidBelow))
                break;

            var newEye = SpawnAtPosition(_zEyeProto, new EntityCoordinates(mapUidBelow.Value, globalPos));

            Transform(newEye).GridTraversal = false;
            _viewSubscriber.AddViewSubscriber(newEye, actor.PlayerSession);
            eyes.Add(newEye);
        }

        // We constantly load the upper z-level for the client so that you can quickly look up and climb stairs without PVS lag.
        if (TryMapUp(map.Value, out var aboveMapUid))
        {
            var newEye = SpawnAtPosition(_zEyeProto, new EntityCoordinates(aboveMapUid.Value, globalPos));

            Transform(newEye).GridTraversal = false;
            _viewSubscriber.AddViewSubscriber(newEye, actor.PlayerSession);
            eyes.Add(newEye);
        }
    }

    private void OnZLevelFall(Entity<CEZPhysicsComponent> ent, ref CEZLevelFallMapEvent args)
    {
        //A dirty trick: we call PredictedPopup on the falling entity on SERVER.
        //This means that the one who is falling does not see the popup itself, but everyone around them does. This is what we need.
        _popup.PopupCoordinates(Loc.GetString("ce-zlevel-falling-popup", ("name", Identity.Name(ent, EntityManager))), Transform(ent).Coordinates, ent);
    }
}
