using Content.Server.GameTicking;
using Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;
using Content.Server.Xenoarchaeology.XenoArtifacts.Events;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Systems;

public sealed class GameRuleArtifactSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly GameTicker _ticker = default!;

    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<GameRuleArtifactComponent, ArtifactActivatedEvent>(OnActivate);
    }

    private void OnActivate(EntityUid uid, GameRuleArtifactComponent component, ArtifactActivatedEvent args)
    {
        foreach (var rule in component.Rules)
            // TODO: Check if this is a valid game rule.
            _ticker.AddGameRule(rule);
    }
}

