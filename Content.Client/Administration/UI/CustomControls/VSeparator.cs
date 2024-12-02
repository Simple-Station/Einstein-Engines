#region

using Robust.Client.Graphics;
using Robust.Client.UserInterface.Controls;

#endregion


namespace Content.Client.Administration.UI.CustomControls;


public sealed class VSeparator : PanelContainer
{
    private static readonly Color SeparatorColor = Color.FromHex("#3D4059");

    public VSeparator(Color color)
    {
        MinSize = new(2, 5);

        AddChild(
            new PanelContainer
            {
                PanelOverride = new StyleBoxFlat
                {
                    BackgroundColor = color
                }
            });
    }

    public VSeparator() : this(SeparatorColor) { }
}
