/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using System.Numerics;
using Content.Server.Chat.Systems;
using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared._CE.ZLevels.Core.EntitySystems;
using Content.Shared.Chat;
using Content.Shared.IdentityManagement;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;

namespace Content.Server._CE.ZLevels.Chat;

public sealed partial class CEZLevelsSpeakingSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly CESharedZLevelsSystem _zLevel = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private EntityQuery<MapComponent> _mapQuery;

    private const float TransmitterLifetime = 3f;
    private const int MessageDelayMilliseconds = 333;

    public override void Initialize()
    {
        base.Initialize();

        _mapQuery = GetEntityQuery<MapComponent>();

        SubscribeLocalEvent<CEZLevelViewerComponent, EntitySpokeEvent>(OnSpoke);
    }

    private void OnSpoke(Entity<CEZLevelViewerComponent> ent, ref EntitySpokeEvent args)
    {
        var xform = Transform(ent);
        var sourceMap = xform.MapUid;
        if (sourceMap is null)
            return;

        if (args.IsWhisper) // curse of chatcode is dead
            return;

        var globalPosition = _transform.GetWorldPosition(xform);
        var message = args.Message;

        //Try transmit message to 1 zlevel down
        if (_zLevel.TryMapDown(sourceMap.Value, out var belowMapUid) &&
            _mapQuery.TryComp(belowMapUid, out var belowMapComp))
        {
            TransmitMessageToZLevel(
                belowMapComp,
                globalPosition,
                message,
                Loc.GetString("ce-zlevel-voice-from-up", ("name", Identity.Name(ent, EntityManager))));
        }

        //Try transmit message to 1 zlevel up
        if (_zLevel.TryMapUp(sourceMap.Value, out var aboveMapUid) &&
            _mapQuery.TryComp(aboveMapUid, out var aboveMapComp))
        {
            TransmitMessageToZLevel(
                aboveMapComp,
                globalPosition,
                message,
                Loc.GetString("ce-zlevel-voice-from-down", ("name", Identity.Name(ent, EntityManager))));
        }
    }

    private void TransmitMessageToZLevel(MapComponent mapComp, Vector2 position, string message, string nameOverride)
    {
        var targetPos = new MapCoordinates(position, mapComp.MapId);
        var transmit = Spawn(null, targetPos);
        EnsureComp<TimedDespawnComponent>(transmit).Lifetime = TransmitterLifetime;

        //It's not the most elegant solution, but as far as I understand, the entity doesn't have time to enter
        //the client's PVS after spawning, and we already start communicating through it. A slight delay solves the problem.
        Timer.Spawn(MessageDelayMilliseconds,
            () =>
            {
                _chat.TrySendInGameICMessage(
                    transmit,
                    message,
                    InGameICChatType.Whisper,
                    false,
                    nameOverride: nameOverride,
                    ignoreActionBlocker: true);
            });
    }
}
