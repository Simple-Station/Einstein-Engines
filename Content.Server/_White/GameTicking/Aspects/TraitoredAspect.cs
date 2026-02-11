using System.Linq;
using Content.Server._White.GameTicking.Aspects.Components;
using Content.Server.Antag;
using Content.Server.Chat.Managers;
using Content.Server.Chat.Systems;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.GameTicking.Rules;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using MindComponent = Content.Shared.Mind.MindComponent;

namespace Content.Server._White.GameTicking.Aspects;

public sealed class TraitoredAspect : AspectSystem<TraitoredAspectComponent>
{
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ChatSystem _chatSystem = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly TraitorRuleSystem _traitorRuleSystem = default!;
    [Dependency] private readonly WhiteGameTicker _whiteGameTicker = default!;
    [Dependency] private readonly AntagSelectionSystem _antag = default!;

    protected override void Started(EntityUid uid, TraitoredAspectComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        var traitorRuleComponents = EntityQuery<TraitorRuleComponent>().ToList();
        if (traitorRuleComponents.Count == 0)
        {
            _whiteGameTicker.RunRandomAspect();
            ForceEndSelf(uid, gameRule);
            return;
        }

        component.TraitorRuleComponent = traitorRuleComponents.First();
        component.AnnouncedForAllAt = _timing.CurTime + _random.Next(component.AnnouncedForAllViaMin, component.AnnouncedForAllViaMax);
        component.AnnouncedForTraitorsAt = _timing.CurTime + component.AnnouncedForTraitorsVia;
    }

    protected override void ActiveTick(EntityUid uid, TraitoredAspectComponent component, GameRuleComponent gameRule, float frameTime)
    {
        base.ActiveTick(uid, component, gameRule, frameTime);

        if (component.TraitorRuleComponent?.SelectionStatus != TraitorRuleComponent.SelectionState.Started)
            return;

        if (!component.AnnouncedForTraitors && _timing.CurTime >= component.AnnouncedForTraitorsAt)
        {
            AnnounceToTraitors(uid, gameRule, component.AnnouncementForTraitorSound);
            component.AnnouncedForTraitors = true;
        }

        if (_timing.CurTime >= component.AnnouncedForAllAt)
            AnnounceToAll(uid, gameRule);
    }

    private void AnnounceToTraitors(EntityUid uid, GameRuleComponent rule, string sound)
    {
        var traitors = new List<Entity<MindComponent>>();
        var query = EntityQueryEnumerator<TraitorRuleComponent>();
        while (query.MoveNext(out var traitorUid, out _))
        {
            traitors.AddRange(_antag.GetAntagMinds(traitorUid));
        }

        if (traitors.Count == 0)
        {
            ForceEndSelf(uid, rule);
            return;
        }

        foreach (var traitor in traitors)
        {
            var mindOwned = traitor.Comp.OwnedEntity;

            if (mindOwned == null || !_mindSystem.TryGetSession(traitor.Owner, out var session))
                continue;

            _chatManager.DispatchServerMessage(session, Loc.GetString("aspect-traitored-briefing"));
            _audio.PlayEntity(sound, mindOwned.Value, mindOwned.Value);
        }
    }

    private void AnnounceToAll(EntityUid uid, GameRuleComponent rule)
    {
        var traitors = new List<Entity<MindComponent>>();
        var query = EntityQueryEnumerator<TraitorRuleComponent>();
        while (query.MoveNext(out var traitorUid, out _))
        {
            traitors.AddRange(_antag.GetAntagMinds(traitorUid));
        }

        var msg = Loc.GetString("aspect-traitored-announce");
        bool foundAny = false;

        foreach (var traitor in traitors)
        {
            var ownedEntity = traitor.Comp.OwnedEntity;

            if (ownedEntity == null)
                continue;

            var name = EntityManager.GetComponent<MetaDataComponent>(ownedEntity.Value).EntityName;

            if (!string.IsNullOrEmpty(name))
            {
                msg += $"\n + {Loc.GetString("aspect-traitored-announce-name", ("name", (object) name))}";
                foundAny = true;
            }
        }

        if (foundAny)
        {
            _chatSystem.DispatchGlobalAnnouncement(msg, Loc.GetString("aspect-traitored-announce-sender"), colorOverride: Color.Aquamarine);
        }

        ForceEndSelf(uid, rule);
    }
}
