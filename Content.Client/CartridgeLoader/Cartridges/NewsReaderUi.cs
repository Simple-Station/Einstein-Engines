#region

using Content.Client.UserInterface.Fragments;
using Content.Shared.CartridgeLoader;
using Content.Shared.CartridgeLoader.Cartridges;
using Robust.Client.UserInterface;

#endregion


namespace Content.Client.CartridgeLoader.Cartridges;


public sealed partial class NewsReaderUi : UIFragment
{
    private NewsReaderUiFragment? _fragment;

    public override Control GetUIFragmentRoot() => _fragment!;

    public override void Setup(BoundUserInterface userInterface, EntityUid? fragmentOwner)
    {
        _fragment = new();

        _fragment.OnNextButtonPressed += () =>
        {
            SendNewsReaderMessage(NewsReaderUiAction.Next, userInterface);
        };
        _fragment.OnPrevButtonPressed += () =>
        {
            SendNewsReaderMessage(NewsReaderUiAction.Prev, userInterface);
        };
        _fragment.OnNotificationSwithPressed += () =>
        {
            SendNewsReaderMessage(NewsReaderUiAction.NotificationSwitch, userInterface);
        };
    }

    public override void UpdateState(BoundUserInterfaceState state)
    {
        switch (state)
        {
            case NewsReaderBoundUserInterfaceState cast:
                _fragment?.UpdateState(cast.Article, cast.TargetNum, cast.TotalNum, cast.NotificationOn);
                break;
            case NewsReaderEmptyBoundUserInterfaceState empty:
                _fragment?.UpdateEmptyState(empty.NotificationOn);
                break;
        }
    }

    private void SendNewsReaderMessage(NewsReaderUiAction action, BoundUserInterface userInterface)
    {
        var newsMessage = new NewsReaderUiMessageEvent(action);
        var message = new CartridgeUiMessage(newsMessage);
        userInterface.SendMessage(message);
    }
}
