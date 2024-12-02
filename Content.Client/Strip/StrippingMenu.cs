#region

using Content.Client.Inventory;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.Timing;
using static Robust.Client.UserInterface.Controls.BoxContainer;

#endregion


namespace Content.Client.Strip;


public sealed class StrippingMenu : DefaultWindow
{
    public LayoutContainer InventoryContainer = new();
    public BoxContainer HandsContainer = new() { Orientation = LayoutOrientation.Horizontal, };
    public BoxContainer SnareContainer = new();
    private readonly StrippableBoundUserInterface _bui;
    public bool Dirty = true;

    public StrippingMenu(string title, StrippableBoundUserInterface bui)
    {
        Title = title;
        _bui = bui;

        var box = new BoxContainer { Orientation = LayoutOrientation.Vertical, Margin = new(0, 8), };
        Contents.AddChild(box);
        box.AddChild(SnareContainer);
        box.AddChild(HandsContainer);
        box.AddChild(InventoryContainer);
    }

    public void ClearButtons()
    {
        InventoryContainer.DisposeAllChildren();
        HandsContainer.DisposeAllChildren();
        SnareContainer.DisposeAllChildren();
    }

    protected override void FrameUpdate(FrameEventArgs args)
    {
        if (!Dirty)
            return;

        Dirty = false;
        _bui.UpdateMenu();
    }
}
