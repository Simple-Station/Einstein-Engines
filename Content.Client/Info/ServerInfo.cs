#region

using Robust.Client.UserInterface.Controls;
using Robust.Shared.Utility;

#endregion


namespace Content.Client.Info;


public sealed class ServerInfo : BoxContainer
{
    private readonly RichTextLabel _richTextLabel;

    public ServerInfo()
    {
        Orientation = LayoutOrientation.Vertical;

        _richTextLabel = new()
        {
            VerticalExpand = true
        };
        AddChild(_richTextLabel);
    }

    public void SetInfoBlob(string markup) => _richTextLabel.SetMessage(FormattedMessage.FromMarkup(markup));
}
