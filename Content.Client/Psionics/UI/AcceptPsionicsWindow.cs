#region

using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using static Robust.Client.UserInterface.Controls.BoxContainer;

#endregion


namespace Content.Client.Psionics.UI;


public sealed class AcceptPsionicsWindow : DefaultWindow
{
    public readonly Button DenyButton;
    public readonly Button AcceptButton;

    public AcceptPsionicsWindow()
    {
        Title = Loc.GetString("accept-psionics-window-title");

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
                                Text = Loc.GetString("accept-psionics-window-prompt-text-part")
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
