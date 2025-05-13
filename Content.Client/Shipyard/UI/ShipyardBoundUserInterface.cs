using Content.Shared.Access.Systems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Shipyard;
using Content.Shared.Whitelist;
using Robust.Client.Player;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Prototypes;

namespace Content.Client.Shipyard.UI;

public sealed class ShipyardConsoleBoundUserInterface : BoundUserInterface
{

    public int Balance { get; private set; }

    public int? ShipSellValue { get; private set; }

    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IPlayerManager _player = default!;

    private readonly AccessReaderSystem _access;
    private readonly EntityWhitelistSystem _whitelist;

    [ViewVariables]
    private ShipyardConsoleMenu? _menu;

    public ShipyardConsoleBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _access = EntMan.System<AccessReaderSystem>();
        _whitelist = EntMan.System<EntityWhitelistSystem>();
    }

    protected override void Open()
    {
        base.Open();

        _menu = new ShipyardConsoleMenu(this);
        _menu.OpenCentered();
        _menu.OnClose += Close;
        _menu.OnPurchased += Purchase;
        _menu.OnSellShip += SellShip;
        _menu.TargetIdButton.OnPressed += _ => SendMessage(new ItemSlotButtonPressedEvent("ShipyardConsole-targetId"));
    }

    private void SellShip(BaseButton.ButtonEventArgs args)
    {
        //reserved for a sanity check, but im not sure what since we check all the important stuffs on server already
        SendMessage(new ShipyardConsoleSellMessage());
    }
    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not ShipyardConsoleInterfaceState cState)
            return;

        Balance = (int)cState.Balance;
        ShipSellValue = cState.ShipSellValue;
        var castState = (ShipyardConsoleInterfaceState) state;
        Populate(castState.ShipyardPrototypes, castState.ShipyardName);
        _menu?.UpdateState(castState);
    }

    private void Populate(List<string> prototypes, string name)
    {
        if (_menu == null)
            return;

        _menu.PopulateProducts(prototypes, name);
        _menu.PopulateCategories();
    }


    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _menu?.Dispose();
    }

    private void Purchase(string id)
    {
        SendMessage(new ShipyardConsolePurchaseMessage(id));
    }
}
