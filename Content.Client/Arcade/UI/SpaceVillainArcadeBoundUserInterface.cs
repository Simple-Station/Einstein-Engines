﻿#region

using static Content.Shared.Arcade.SharedSpaceVillainArcadeComponent;

#endregion


namespace Content.Client.Arcade.UI;


public sealed class SpaceVillainArcadeBoundUserInterface : BoundUserInterface
{
    [ViewVariables] private SpaceVillainArcadeMenu? _menu;

    //public SharedSpaceVillainArcadeComponent SpaceVillainArcade;

    public SpaceVillainArcadeBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        SendAction(PlayerAction.RequestData);
    }

    public void SendAction(PlayerAction action) => SendMessage(new SpaceVillainArcadePlayerActionMessage(action));

    protected override void Open()
    {
        base.Open();

        _menu = new(this);

        _menu.OnClose += Close;
        _menu.OpenCentered();
    }

    protected override void ReceiveMessage(BoundUserInterfaceMessage message)
    {
        if (message is SpaceVillainArcadeDataUpdateMessage msg)
            _menu?.UpdateInfo(msg);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _menu?.Dispose();
    }
}
