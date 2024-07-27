using Content.Client.UserInterface.Fragments;
using Content.Shared.CartridgeLoader.Cartridges;
using Robust.Client.UserInterface;

namespace Content.Client.CartridgeLoader.Cartridges;

public sealed partial class NanoMessageUi : UIFragment
{
    private NanoMessageUiFragment? _fragment;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new NanoMessageUiFragment();
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not NanoMessageUiState uiState)
            return;

        _fragment?.UpdateState(uiState);
    }
}
