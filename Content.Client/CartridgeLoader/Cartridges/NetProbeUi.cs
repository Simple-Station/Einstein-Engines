﻿#region

using Content.Client.UserInterface.Fragments;
using Content.Shared.CartridgeLoader.Cartridges;
using Robust.Client.UserInterface;

#endregion


namespace Content.Client.CartridgeLoader.Cartridges;


public sealed partial class NetProbeUi : UIFragment
{
    private NetProbeUiFragment? _fragment;

    public override Control GetUIFragmentRoot() => _fragment!;

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner) => _fragment = new();

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not NetProbeUiState netProbeUiState)
            return;

        _fragment?.UpdateState(netProbeUiState.ProbedDevices);
    }
}
