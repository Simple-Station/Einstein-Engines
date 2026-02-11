using Content.Server._White.GameTicking.Aspects.Components;
using Content.Server.Administration.Logs;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules;
using Content.Shared.Database;
using Content.Shared.GameTicking.Components;
using Robust.Server.Audio;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._White.GameTicking.Aspects;

/// <summary>
/// Base class for aspect systems.
/// </summary>
/// <typeparam name="T">The type of component to which the system is applied.</typeparam>
public abstract class AspectSystem<T> : GameRuleSystem<T> where T : Component
{
    [Dependency] private readonly IAdminLogManager _adminLogManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;

    protected ISawmill Sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        Sawmill = Logger.GetSawmill("aspects");
    }

    /// <summary>
    /// Called every tick when this aspect is running.
    /// </summary>
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<GameRuleComponent>();
        while (query.MoveNext(out var uid, out var ruleData))
        {
            if (!HasComp<AspectComponent>(uid) || !GameTicker.IsGameRuleAdded(uid, ruleData))
                continue;

            if (!GameTicker.IsGameRuleActive(uid, ruleData) && _timing.CurTime >= ruleData.ActivatedAt)
                GameTicker.StartGameRule(uid, ruleData);
        }
    }

    /// <summary>
    /// Called when an aspect is added to an entity.
    /// </summary>
    protected override void Added(EntityUid uid, T component, GameRuleComponent gameRule, GameRuleAddedEvent args)
    {
        base.Added(uid, component, gameRule, args);

        if (!TryComp<AspectComponent>(uid, out var aspect))
            return;

        _adminLogManager.Add(LogType.AspectAnnounced, $"{Loc.GetString("aspect-admin-log-added", ("aspect", ToPrettyString(uid)))}");

        if (!aspect.IsHidden)
            _chatSystem.DispatchGlobalAnnouncement(Loc.GetString(Description(uid)), playSound: false, colorOverride: Color.Aquamarine);

        _audio.PlayGlobal(aspect.StartAudio, Filter.Broadcast(), true);
    }

    /// <summary>
    /// Called when an aspect is started.
    /// </summary>
    protected override void Started(
        EntityUid uid,
        T component,
        GameRuleComponent gameRule,
        GameRuleStartedEvent args
        )
    {
        base.Started(uid, component, gameRule, args);

        if (!HasComp<AspectComponent>(uid))
            return;

        _adminLogManager.Add(LogType.AspectStarted, LogImpact.High, $"{Loc.GetString("aspect-admin-log-started", ("aspect", ToPrettyString(uid)))}");
    }

    /// <summary>
    /// Called when an aspect is ended.
    /// </summary>
    protected override void Ended(EntityUid uid, T component, GameRuleComponent gameRule, GameRuleEndedEvent args)
    {
        base.Ended(uid, component, gameRule, args);

        if (!TryComp<AspectComponent>(uid, out var aspect))
            return;

        _adminLogManager.Add(LogType.AspectStopped, $"{Loc.GetString("aspect-admin-log-ended", ("aspect", ToPrettyString(uid)))}");

        if (!aspect.IsHidden)
            _chatSystem.DispatchGlobalAnnouncement($"{Loc.GetString("aspect-announcement-ended", ("aspect", Name(uid)))}", playSound: false, colorOverride: Color.Aquamarine);

        _audio.PlayGlobal(aspect.EndAudio, Filter.Broadcast(), true);
    }

    /// <summary>
    /// Forces this aspect to end prematurely.
    /// </summary>
    /// <param name="uid">The entity UID on which the aspect is being performed.</param>
    /// <param name="component">The game rule component associated with this aspect (optional).</param>
    protected new void ForceEndSelf(EntityUid uid, GameRuleComponent? component = null)
    {
        GameTicker.EndGameRule(uid, component);
    }
}
