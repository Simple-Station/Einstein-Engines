using Content.Client.UserInterface.Fragments;
using Content.Shared.CartridgeLoader;
using Content.Shared.CartridgeLoader.Cartridges;
using Content.Shared.NanoMessage.Events;
using Content.Shared.NanoMessage.Events.Cartridge;
using Robust.Client.UserInterface;

namespace Content.Client.CartridgeLoader.Cartridges;

public sealed partial class NanoMessageUi : UIFragment
{
    private NanoMessage.NanoMessageUiFragment? _fragment;
    private BoundUserInterface? _bui;

    public override Control GetUIFragmentRoot()
    {
        return _fragment!;
    }

    public override void Setup(BoundUserInterface bui, EntityUid? fragmentOwner)
    {
        _bui = bui;
        _fragment = new NanoMessage.NanoMessageUiFragment();

        _fragment.OnRecipientAdd += id => SendMessage(new NanoMessageCartridgeAddRecipientRequest { Id = id });

        _fragment.OnRecipientChoose += id => SendMessage(new NanoMessageChooseConversationRequest { RecipientId = id });

        _fragment.OnRefreshServer += () => SendMessage(new NanoMessageReconnectRequest());

        _fragment.OnMessageSend += (id, message) =>
            SendMessage(new NanoMessageMessageSendRequest { RecipientId = id, Message = message });
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not NanoMessageUiState uiState)
            return;

        _fragment?.UpdateState(uiState);
    }

    // Why does UiFragment not include this?
    private void SendMessage(CartridgeMessageEvent ev)
    {
        _bui?.SendMessage(new CartridgeUiMessage(ev));
    }
}
