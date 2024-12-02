#region

using Robust.Client.UserInterface;
using Robust.Client.UserInterface.XAML;

#endregion


namespace Content.Client.Hands.UI;


public sealed class HandVirtualItemStatus : Control
{
    public HandVirtualItemStatus()
    {
        RobustXamlLoader.Load(this);
    }
}
