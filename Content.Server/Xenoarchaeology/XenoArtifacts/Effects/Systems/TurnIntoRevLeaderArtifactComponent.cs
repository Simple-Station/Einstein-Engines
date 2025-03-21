using Content.Server.Antag;
using Content.Server.GameTicking.Rules.Components;
using Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Components;
using Content.Server.Xenoarchaeology.XenoArtifacts.Events;
using Content.Shared.Mind.Components;
using Robust.Shared.Player;

namespace Content.Server.Xenoarchaeology.XenoArtifacts.Effects.Systems;

public sealed class TurnIntoRevLeaderArtifactSystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;

    const string DefaultRevleaderRule = "Revolutionary";
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<TurnIntoRevLeaderArtifactComponent, ArtifactActivatedEvent>(OnActivate);
    }

    private void OnActivate(EntityUid uid, TurnIntoRevLeaderArtifactComponent comp, ArtifactActivatedEvent args)
    {
        if (!HasComp<MindContainerComponent>(args.Activator) || !TryComp<ActorComponent>(args.Activator, out var target))
            return;

        if (HasComp<RevolutionaryRuleComponent>(args.Activator))
            return;

        var player = target.PlayerSession;

        _antag.ForceMakeAntag<RevolutionaryRuleComponent>(player, comp.Rule ?? DefaultRevleaderRule);
    }
}
