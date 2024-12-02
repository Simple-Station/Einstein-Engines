#region

using Content.Client.Eui;

#endregion


namespace Content.Client.Revolutionary.UI;


public sealed class DeconvertedEui : BaseEui
{
    private readonly DeconvertedMenu _menu;

    public DeconvertedEui()
    {
        _menu = new();
    }

    public override void Opened() => _menu.OpenCentered();

    public override void Closed()
    {
        base.Closed();

        _menu.Close();
    }
}
