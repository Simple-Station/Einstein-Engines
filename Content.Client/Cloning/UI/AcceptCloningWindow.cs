#region

using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using static Robust.Client.UserInterface.Controls.BoxContainer;

#endregion


namespace Content.Client.Cloning.UI;


public sealed class AcceptCloningWindow : DefaultWindow
{
    public readonly Button DenyButton;
    public readonly Button AcceptButton;

    public AcceptCloningWindow()
    {
        Title = Loc.GetString("accept-cloning-window-title");

        Contents.AddChild(
            new BoxContainer
            {
                Orientation = LayoutOrientation.Vertical,
                Children =
                {
                    new BoxContainer
                    {
                        Orientation = LayoutOrientation.Vertical,
                        Children =
                        {
                            new Label
                            {
                                Text = Loc.GetString("accept-cloning-window-prompt-text-part")
                            },
                            new BoxContainer
                            {
                                Orientation = LayoutOrientation.Horizontal,
                                Align = AlignMode.Center,
                                Children =
                                {
                                    (AcceptButton = new()
                                    {
                                        Text = Loc.GetString("accept-cloning-window-accept-button")
                                    }),

                                    new()
                                    {
                                        MinSize = new(20, 0)
                                    },

                                    (DenyButton = new()
                                    {
                                        Text = Loc.GetString("accept-cloning-window-deny-button")
                                    })
                                }
                            }
                        }
                    }
                }
            });
    }
}
