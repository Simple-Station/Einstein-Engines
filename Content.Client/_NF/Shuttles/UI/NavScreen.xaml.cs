// New Frontiers - This file is licensed under AGPLv3
// Copyright (c) 2024 New Frontiers Contributors
// See AGPLv3.txt for details.
using Content.Shared._NF.Shuttles.Events;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Shuttles.UI;

public sealed partial class NavScreen
{
    private readonly ButtonGroup _buttonGroup = new();
    public event Action<NetEntity?, InertiaDampeningMode>? OnInertiaDampeningModeChanged;

    private void NfInitialize()
    {
        DampenerOff.OnPressed += _ => SetDampenerMode(InertiaDampeningMode.Off);
        DampenerOn.OnPressed += _ => SetDampenerMode(InertiaDampeningMode.Dampen);
        AnchorOn.OnPressed += _ => SetDampenerMode(InertiaDampeningMode.Anchor);

        DampenerOff.Group = _buttonGroup;
        DampenerOn.Group = _buttonGroup;
        AnchorOn.Group = _buttonGroup;

        // Send off a request to get the current dampening mode.
        _entManager.TryGetNetEntity(_shuttleEntity, out var shuttle);
        OnInertiaDampeningModeChanged?.Invoke(shuttle, InertiaDampeningMode.Query);
    }

    private void SetDampenerMode(InertiaDampeningMode mode)
    {
        NavRadar.DampeningMode = mode;
        _entManager.TryGetNetEntity(_shuttleEntity, out var shuttle);
        OnInertiaDampeningModeChanged?.Invoke(shuttle, mode);
    }

    private void NfUpdateState()
    {
        if (NavRadar.DampeningMode == InertiaDampeningMode.Station)
            DampenerModeButtons.Visible = false;
        else
        {
            DampenerModeButtons.Visible = true;
            DampenerOff.Pressed = NavRadar.DampeningMode == InertiaDampeningMode.Off;
            DampenerOn.Pressed = NavRadar.DampeningMode == InertiaDampeningMode.Dampen;
            AnchorOn.Pressed = NavRadar.DampeningMode == InertiaDampeningMode.Anchor;
        }
    }
}
