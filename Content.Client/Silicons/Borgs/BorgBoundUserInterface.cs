#region

using Content.Shared.Silicons.Borgs;
using JetBrains.Annotations;

#endregion


namespace Content.Client.Silicons.Borgs;


[UsedImplicitly]
public sealed class BorgBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private BorgMenu? _menu;

    public BorgBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();

        var owner = Owner;

        _menu = new(owner);

        _menu.BrainButtonPressed += () =>
        {
            SendMessage(new BorgEjectBrainBuiMessage());
        };

        _menu.EjectBatteryButtonPressed += () =>
        {
            SendMessage(new BorgEjectBatteryBuiMessage());
        };

        _menu.NameChanged += name =>
        {
            SendMessage(new BorgSetNameBuiMessage(name));
        };

        _menu.RemoveModuleButtonPressed += module =>
        {
            SendMessage(new BorgRemoveModuleBuiMessage(EntMan.GetNetEntity(module)));
        };

        _menu.OnClose += Close;

        _menu.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not BorgBuiState msg)
            return;
        _menu?.UpdateState(msg);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;
        _menu?.Dispose();
    }
}
