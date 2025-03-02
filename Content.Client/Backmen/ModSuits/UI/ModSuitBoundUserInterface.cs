using Content.Shared.Backmen.ModSuits;
using Robust.Client.UserInterface;

namespace Content.Client.Backmen.ModSuits.UI;

public sealed class ModSuitBoundUserInterface : BoundUserInterface
{
    private readonly IEntityManager _entityManager;
    private ModSuitRadialMenu? _menu;

    public ModSuitBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
        _entityManager = IoCManager.Resolve<IEntityManager>();
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<ModSuitRadialMenu>();
        _menu.SetEntity(Owner);
        _menu.SendToggleClothingMessageAction += SendModSuitMessage;

        _menu.OpenCentered();
    }

    private void SendModSuitMessage(EntityUid uid)
    {
        var message = new ModSuitUiMessage(_entityManager.GetNetEntity(uid));
        SendPredictedMessage(message);
    }
}
