#region

using Content.Client.Eui;
using Content.Shared.Eui;

#endregion


namespace Content.Client.NPC;


public sealed class NPCEui : BaseEui
{
    private NPCWindow? _window = new();

    public override void Opened()
    {
        base.Opened();
        _window = new();
        _window.OpenCentered();
        _window.OnClose += OnClosed;
    }

    private void OnClosed() => SendMessage(new CloseEuiMessage());

    public override void Closed()
    {
        base.Closed();
        _window?.Close();
        _window = null;
    }
}
