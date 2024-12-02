#region

using Content.Client.Stylesheets;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

#endregion


namespace Content.Client.UserInterface.Controls;


public sealed class HighDivider : Control
{
    public HighDivider()
    {
        Children.Add(new PanelContainer { StyleClasses = { StyleBase.ClassHighDivider, }, });
    }
}
