// New Frontiers - This file is licensed under AGPLv3
// Copyright (c) 2024 New Frontiers Contributors
// See AGPLv3.txt for details.
using Content.Shared._NF.Shuttles.Events;
using Content.Shared.Shuttles.BUIStates;
using Robust.Shared.Physics.Components;
using System.Numerics;
using Robust.Client.Graphics;
using Robust.Shared.Collections;

namespace Content.Client.Shuttles.UI;

public sealed partial class ShuttleNavControl
{
    public InertiaDampeningMode DampeningMode { get; set; }

    private void NfUpdateState(NavInterfaceState state)
    {

        if (!EntManager.GetCoordinates(state.Coordinates).HasValue ||
            !EntManager.TryGetComponent(
                EntManager.GetCoordinates(state.Coordinates).GetValueOrDefault().EntityId,
                out TransformComponent? transform) ||
            !EntManager.TryGetComponent(transform.GridUid, out PhysicsComponent? _))
            return;

        DampeningMode = state.DampeningMode;
    }
}

