#region

using Content.Client.UserInterface.Fragments;
using Content.Shared.CartridgeLoader;
using Content.Shared.CartridgeLoader.Cartridges;
using Robust.Client.UserInterface;

#endregion


namespace Content.Client.CartridgeLoader.Cartridges;


public sealed partial class GlimmerMonitorUi : UIFragment
{
    private GlimmerMonitorUiFragment? _fragment;

    public override Control GetUIFragmentRoot() => _fragment!;

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new();

        _fragment.OnSync += _ => SendSyncMessage(userInterface);
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not GlimmerMonitorUiState monitorState)
            return;

        _fragment?.UpdateState(monitorState.GlimmerValues);
    }

    private void SendSyncMessage(BoundUserInterface userInterface)
    {
        var syncMessage = new GlimmerMonitorSyncMessageEvent();
        var message = new CartridgeUiMessage(syncMessage);
        userInterface.SendMessage(message);
    }
}
