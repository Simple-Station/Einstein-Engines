#region

using Robust.Client.UserInterface.Controls;

#endregion


namespace Content.Client.UserInterface.Controls;


public sealed class Placeholder : PanelContainer
{
    public const string StyleClassPlaceholderText = "PlaceholderText";

    private readonly Label _label;

    public string? PlaceholderText
    {
        get => _label.Text;
        set => _label.Text = value;
    }

    public Placeholder()
    {
        _label = new()
        {
            VerticalAlignment = VAlignment.Stretch,
            Align = Label.AlignMode.Center,
            VAlign = Label.VAlignMode.Center
        };
        _label.AddStyleClass(StyleClassPlaceholderText);
        AddChild(_label);
    }
}
