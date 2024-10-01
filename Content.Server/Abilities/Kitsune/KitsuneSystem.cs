using Content.Shared.Actions.Events;
using Content.Shared.Abilities.Psionics;

namespace Content.Server.Abilities.Kitsune;

public sealed class KitsuneSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PsionicComponent, CreateFoxfireActionEvent>(OnCreateFoxfire);
    }

    // TODO: Make Foxfire work on a TimedDespawnComponent rather than Charges. Which will apparently require I refactor Timed Despawn so it isn't FUCKING FRAMES.
    private void OnCreateFoxfire(EntityUid uid, PsionicComponent component, CreateFoxfireActionEvent args)
    {
        if (HasComp<PsionicInsulationComponent>(uid))
            return;

        var fireEnt = Spawn(args.FoxfirePrototype, Transform(uid).Coordinates);
        var fireComp = EnsureComp<FoxFireComponent>(fireEnt);
        fireComp.Owner = uid;

        args.Handled = true;
    }
}