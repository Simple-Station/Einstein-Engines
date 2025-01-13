// NeuPanda - This file is licensed under AGPLv3
// Copyright (c) 2025 NeuPanda
// See AGPLv3.txt for details.
using Content.Shared._NF.Shuttles.Events;
using Robust.Client.UserInterface.Controls;

namespace Content.Client.Shuttles.UI
{
    public sealed partial class NavScreen
    {
        private readonly ButtonGroup _buttonGroup = new();
        public event Action<NetEntity?, InertiaDampeningMode>? OnInertiaDampeningModeChanged;

        private void NfInitialize()
        {

            DampenerOff.OnPressed += _ => SwitchDampenerMode(InertiaDampeningMode.Off);
            DampenerOn.OnPressed += _ => SwitchDampenerMode(InertiaDampeningMode.Dampened);
            AnchorOn.OnPressed += _ => SwitchDampenerMode(InertiaDampeningMode.Anchored);

            var group = new ButtonGroup();
            DampenerOff.Group = group;
            DampenerOn.Group = group;
            AnchorOn.Group = group;
        }

        private void SwitchDampenerMode(InertiaDampeningMode mode)
        {
            NavRadar.DampeningMode = mode;
            _entManager.TryGetNetEntity(_shuttleEntity, out var shuttle);
            OnInertiaDampeningModeChanged?.Invoke(shuttle, mode);
        }

        private void NfUpdateState()
        {
            DampenerModeButtons.Visible = true;
            DampenerOff.Pressed = NavRadar.DampeningMode == InertiaDampeningMode.Off;
            DampenerOn.Pressed = NavRadar.DampeningMode == InertiaDampeningMode.Dampened;
            AnchorOn.Pressed = NavRadar.DampeningMode == InertiaDampeningMode.Anchored;
        }
    }
}
