using Content.Shared.Backmen.ModSuits;
using Robust.Client.UserInterface;

namespace Content.Client.Backmen.ModSuits.UI;

public sealed class ModSuitDeployBoundUserInterface : BoundUserInterface
{
    private readonly IEntityManager _entityManager;
    private ModSuitDeployRadialMenu? _menu;

    public ModSuitDeployBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
        _entityManager = IoCManager.Resolve<IEntityManager>();
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<ModSuitDeployRadialMenu>();
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
