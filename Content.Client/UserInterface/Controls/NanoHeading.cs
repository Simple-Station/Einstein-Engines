#region

using Content.Client.Stylesheets;
using Robust.Client.UserInterface.Controls;

#endregion


namespace Content.Client.UserInterface.Controls;


public sealed class NanoHeading : Container
{
    private readonly Label _label;
    private readonly PanelContainer _panel;

    public NanoHeading()
    {
        _panel = new()
        {
            Children =
            {
                (_label = new()
                {
                    StyleClasses = { StyleNano.StyleClassLabelHeading, }
                })
            }
        };
        AddChild(_panel);

        HorizontalAlignment = HAlignment.Left;
    }

    public string? Text
    {
        get => _label.Text;
        set => _label.Text = value;
    }
}
