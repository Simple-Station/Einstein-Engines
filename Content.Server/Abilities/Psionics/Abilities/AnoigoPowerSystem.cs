using Content.Shared.Abilities.Psionics;
using Content.Shared.Actions.Events;
using Content.Shared.Psionics.Glimmer;

namespace Content.Server.Abilities.Psionics;


public sealed class AnoigoPowerSystem : EntitySystem
{
    [Dependency] private readonly GlimmerSystem _glimmer = default!;
    [Dependency] private readonly SharedPsionicAbilitiesSystem _psionics = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PsionicComponent, AnoigoPowerActionEvent>(OnPowerUsed);
    }

    private void OnPowerUsed(EntityUid uid,PsionicComponent component, AnoigoPowerActionEvent args)
    {
        if (!_psionics.OnAttemptPowerUse(args.Performer, "anoigo"))
            return;

        Anoigo(args.Performer, args.Target);
        _psionics.LogPowerUsed(args.Performer, "anoigo");
        args.Handled = true;
    }
    private void Anoigo(EntityUid Performer, EntityUid Target)
    {

    }
}
