using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;
using Content.Server.Xenoarchaeology.XenoArtifacts.Events;
using Content.Shared.Mind.Components;
using Robust.Shared.Player;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Systems;

public sealed class TurnIntoThiefArtifactSystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;

    const string DefaultTraitorRule = "Thief";
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<TurnIntoThiefArtifactComponent, ArtifactActivatedEvent>(OnActivate);
    }

    private void OnActivate(EntityUid uid, TurnIntoThiefArtifactComponent comp, ArtifactActivatedEvent args)
    {
        if (!HasComp<MindContainerComponent>(args.Activator) || !TryComp<ActorComponent>(args.Activator, out var target))
            return;

        if (HasComp<ThiefRuleComponent>(args.Activator))
            return;

        var player = target.PlayerSession;

        _antag.ForceMakeAntag<ThiefRuleComponent>(player, comp.Rule ?? DefaultTraitorRule);
    }
}
