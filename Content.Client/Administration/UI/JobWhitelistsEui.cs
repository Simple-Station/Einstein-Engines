#region

using Content.Client.Eui;
using Content.Shared.Administration;
using Content.Shared.Eui;

#endregion


namespace Content.Client.Administration.UI;


public sealed class JobWhitelistsEui : BaseEui
{
    private readonly JobWhitelistsWindow Window;

    public JobWhitelistsEui()
    {
        Window = new();
        Window.OnClose += () => SendMessage(new CloseEuiMessage());
        Window.OnSetJob += (id, whitelisted) => SendMessage(new SetJobWhitelistedMessage(id, whitelisted));
    }

    public override void HandleState(EuiStateBase state)
    {
        if (state is not JobWhitelistsEuiState cast)
            return;

        Window.HandleState(cast);
    }

    public override void Opened()
    {
        base.Opened();

        Window.OpenCentered();
    }

    public override void Closed()
    {
        base.Closed();

        Window.Close();
        Window.Dispose();
    }
}
