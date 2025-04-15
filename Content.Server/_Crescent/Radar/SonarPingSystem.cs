using System.Collections.Frozen;
using System.Linq;
using System.Numerics;
using Content.Server.Chat.Systems;
using Content.Server.Power.Components;
using Content.Server.Radio.EntitySystems;
using Content.Shared.Chat;
using Content.Shared.Crescent.Radar;
using Content.Shared.Shuttles.BUIStates;
using Content.Shared.Shuttles.Components;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Timing;

namespace Content.Server.Crescent.Radar;

/// <summary>
/// This handles...
/// </summary>
public sealed class SonarPingSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    [Dependency] private readonly IGameTiming _timer = default!;
    private Dictionary<EntityUid, HashSet<EntityUid>> receptionList = new();

    private float curTime = 0f;
    private const float pingCheckInterval = 5f;
    private TimeSpan alertCooldown = TimeSpan.FromSeconds(10);

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<RadarDetectorComponent, GetVerbsEvent<ActivationVerb>>(RequestVerbs);
    }

    public void RequestVerbs(EntityUid owner, RadarDetectorComponent comp, ref GetVerbsEvent<ActivationVerb> args)
    {

        ActivationVerb verb = new()
        {
            Text = $"Toggle Alert Pinging",
            Act = () =>
            {
                comp.alertOnPing = !comp.alertOnPing;
            }
        };
        args.Verbs.Add(verb);

    }
    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (_timer.IsFirstTimePredicted)
            curTime += frameTime;
        var query = EntityQueryEnumerator<RadarConsoleComponent, RadarDetectorComponent, TransformComponent>();
        var checking = EntityManager.GetAllComponents(typeof(RadarPingerComponent), false).ToDictionary();
        while (query.MoveNext(out var uid, out var radar, out var pinger, out var transform))
        {
            var ourRange = radar.MaxRange * 0.8;
            var ourPos = _transform.GetWorldPosition(transform);
            if (!receptionList.ContainsKey(uid))
            {
                receptionList.Add(uid, new HashSet<EntityUid>());
            }

            var ourHash = receptionList[uid];

            foreach (var (key, _) in checking)
            {
                var targetTrans = Transform(key);
                // dont care about inactives
                if (!_uiSystem.IsUiOpen(key, RadarConsoleUiKey.Key))
                    continue;
                if ((_transform.GetWorldPosition(targetTrans) - ourPos).Length() > ourRange)
                    continue;
                ourHash.Add(key);
            }

        }
        if (curTime > pingCheckInterval)
        {
            curTime = 0f;

            var worldTime = _timer.CurTime;
            foreach(var (key, set) in receptionList)
            {
                if (!TryComp<RadarDetectorComponent>(key, out var comp))
                    continue;
                if (!TryComp<ApcPowerReceiverComponent>(key ,out var powerComp))
                    continue;
                if (!powerComp.Powered)
                    continue;
                if (worldTime - comp.lastAlert < TimeSpan.Zero)
                    continue;
                if (!set.Any())
                    continue;
                var closest = 9999f;
                var ourPos = _transform.GetWorldPosition(key);
                foreach (var entity in set)
                {
                    var distance = (_transform.GetWorldPosition(entity) - ourPos).Length();
                    if (distance > closest)
                        continue;
                    closest = distance;
                }

                var message = $"Notice: Mass scanner pings detected in local space! Detecting {set.Count()} scanners! Closest scanner at {Math.Round(closest)} meters!";
                _chatSystem.TrySendInGameICMessage(key, message, InGameICChatType.Speak, ChatTransmitRange.Normal);
                comp.lastAlert = worldTime + alertCooldown;
            }
            receptionList.Clear();

        }


    }
}
