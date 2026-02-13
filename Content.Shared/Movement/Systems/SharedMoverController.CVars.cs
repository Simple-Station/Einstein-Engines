using Content.Shared.CCVar;
using Content.Shared.Mind.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Robust.Shared.Configuration;

namespace Content.Shared.Movement.Systems;

public abstract partial class SharedMoverController
{
    [Dependency] private readonly INetConfigurationManager _netConfig = default!;

    private void InitializeCVars()
    {
        SubscribeLocalEvent<InputMoverComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<InputMoverComponent, MindRemovedMessage>(OnMindRemoved);
        SubscribeNetworkEvent<UpdateInputCVarsMessage>(OnUpdateCVars);
    }

    private void OnMindAdded(Entity<InputMoverComponent> ent, ref MindAddedMessage args)
    {
        if (args.Mind.Comp.Session?.Channel is not { } channel)
            return;

        ent.Comp.DefaultSprinting = _netConfig.GetClientCVar(channel, CCVars.DefaultWalk);
        WalkingAlert(ent);
    }

    private void OnMindRemoved(Entity<InputMoverComponent> ent, ref MindRemovedMessage args)
    {
        // If it's an ai-controlled mob, we probably want them sprinting by default.
        ent.Comp.DefaultSprinting = true;
    }

    private void OnUpdateCVars(UpdateInputCVarsMessage msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } uid || !TryComp<InputMoverComponent>(uid, out var mover))
            return;

        mover.DefaultSprinting = _netConfig.GetClientCVar(args.SenderSession.Channel, CCVars.DefaultWalk);
        WalkingAlert((uid, mover));
    }
}
