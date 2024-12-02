#region

using Content.Client.UserInterface.Fragments;
using Content.Shared.CartridgeLoader.Cartridges;
using Robust.Client.UserInterface;

#endregion


namespace Content.Client.DeltaV.CartridgeLoader.Cartridges;


public sealed partial class SecWatchUi : UIFragment
{
    private SecWatchUiFragment? _fragment;

    public override Control GetUIFragmentRoot() => _fragment!;

    public override void Setup(BoundUserInterface ui, EntityUid? owner) => _fragment = new();

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is SecWatchUiState cast)
            _fragment?.UpdateState(cast);
    }
}
