using Content.Server._White.GameTicking.Aspects.Components;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Mind;
using Content.Server.Store.Systems;
using Content.Server.Traitor.Uplink;
using Content.Shared.FixedPoint;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind;
using MindComponent = Content.Shared.Mind.MindComponent;

namespace Content.Server._White.GameTicking.Aspects;

public sealed class RichTraitorAspect : AspectSystem<RichTraitorAspectComponent>
{
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly TraitorRuleSystem _traitorRuleSystem = default!;
    [Dependency] private readonly UplinkSystem _uplinkSystem = default!;
    [Dependency] private readonly WhiteGameTicker _whiteGameTicker = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;

    protected override void Started(EntityUid uid, RichTraitorAspectComponent component, GameRuleComponent gameRule, GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        if (!_gameTicker.IsGameRuleAdded<TraitorRuleComponent>())
        {
            _whiteGameTicker.RunRandomAspect();
            ForceEndSelf(uid, gameRule);
            return;
        }

        var query = EntityQueryEnumerator<TraitorRuleComponent>();
        while (query.MoveNext(out var traitorRule))
        {
            foreach (var mindId in traitorRule.TraitorMinds)
            {
                if (!TryComp<Content.Shared.Mind.MindComponent>(mindId, out var mind))
                    continue;

                if (mind.OwnedEntity is not { } ent || mind.UserId == null)
                    continue;

                if (_uplinkSystem.FindUplinkTarget(ent) is not { } uplink)
                    continue;

                if (_store.TryAddCurrency(new Dictionary<string, FixedPoint2> { { UplinkSystem.TelecrystalCurrencyPrototype, 10 } }, uplink))
                {
                    if (_mindSystem.TryGetSession(mindId, out var session))
                    {
                        _chatManager.DispatchServerMessage(session, Loc.GetString("aspect-traitor-rich-briefing"));
                    }
                }
            }
        }
    }
}
