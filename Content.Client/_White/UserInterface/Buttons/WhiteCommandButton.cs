using Robust.Client.Console;

namespace Content.Client._White.UserInterface.Buttons;

[Virtual]
public class WhiteCommandButton : WhiteLobbyTextButton
{
    public string? Command { get; set; }

    public WhiteCommandButton()
    {
        OnPressed += Execute;
    }

    private bool CanPress()
    {
        return string.IsNullOrEmpty(Command) ||
               IoCManager.Resolve<IClientConGroupController>().CanCommand(Command.Split(' ')[0]);
    }

    protected override void EnteredTree()
    {
        if (!CanPress())
        {
            Visible = false;
        }
    }

    protected virtual void Execute(ButtonEventArgs obj)
    {
        if (!string.IsNullOrEmpty(Command))
            IoCManager.Resolve<IClientConsoleHost>().ExecuteCommand(Command);
    }
}
