// NeuPanda - This file is licensed under AGPLv3
// Copyright (c) 2025 NeuPanda
// See AGPLv3.txt for details.
using Content.Server.Shuttles.Components;
using Content.Shared._NF.Shuttles.Events;
using Content.Shared.CCVar;
using Robust.Shared.Physics.Components;

namespace Content.Server.Shuttles.Systems;

public sealed partial class ShuttleSystem
{
    private float _spaceFrictionStrength;
    private float _anchorDampeningStrength;
    private void NfInitialize()
    {
        SubscribeLocalEvent<ShuttleConsoleComponent, SetInertiaDampeningRequest>(OnSetInertiaDampening);
        _spaceFrictionStrength = _cfg.GetCVar(CCVars.SpaceFrictionStrength);
        _anchorDampeningStrength = _cfg.GetCVar(CCVars.AnchorDampeningStrength);
    }

    private void OnSetInertiaDampening(EntityUid uid, ShuttleConsoleComponent component, SetInertiaDampeningRequest args)
    {
        if (!EntityManager.TryGetComponent(GetEntity(args.ShuttleEntityUid), out TransformComponent? transform) ||
            !transform.GridUid.HasValue ||
            !EntityManager.TryGetComponent(transform.GridUid, out PhysicsComponent? physicsComponent) ||
            !EntityManager.TryGetComponent(transform.GridUid, out ShuttleComponent? shuttleComponent))
        {
            return;
        }
        _console.RefreshShuttleConsoles(transform.GridUid.Value);

        var linearDampeningStrength = args.Mode switch
        {
            InertiaDampeningMode.Off => _spaceFrictionStrength,
            InertiaDampeningMode.Dampened => shuttleComponent.LinearDamping, // should i use Dampener Strength instead?
            InertiaDampeningMode.Anchored => _anchorDampeningStrength,
            _ => shuttleComponent.LinearDamping, // if some how we end up here... just keep calm and carry on with your bad self
        };

        var angularDampeningStrength = args.Mode switch
        {
            InertiaDampeningMode.Off => _spaceFrictionStrength,
            InertiaDampeningMode.Dampened => shuttleComponent.AngularDamping,
            InertiaDampeningMode.Anchored => _anchorDampeningStrength,
            _ => shuttleComponent.AngularDamping, // if some how we end up here... just keep calm and carry on with your bad self
        };

        _physics.SetLinearDamping(transform.GridUid.Value, physicsComponent, linearDampeningStrength);
        _physics.SetAngularDamping(transform.GridUid.Value, physicsComponent, angularDampeningStrength);
        _console.RefreshShuttleConsoles(transform.GridUid.Value);
    }

    public InertiaDampeningMode NfGetInertiaDampeningMode(EntityUid entity)
    {
        if (!EntityManager.TryGetComponent<TransformComponent>(entity, out var xform) ||
            !EntityManager.TryGetComponent(xform.GridUid, out PhysicsComponent? physicsComponent))
            return InertiaDampeningMode.Dampened;

        if (physicsComponent.LinearDamping == _anchorDampeningStrength)
            return InertiaDampeningMode.Anchored;

        else if (physicsComponent.LinearDamping == _spaceFrictionStrength)
            return InertiaDampeningMode.Off;
        else
            return InertiaDampeningMode.Dampened;
    }
}
