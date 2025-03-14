using Content.Shared.Backmen.ModSuits;
using Robust.Client.UserInterface;

namespace Content.Client.Backmen.ModSuits.UI;

public sealed class ModSuitBoundUserInterface : BoundUserInterface
{
    private readonly IEntityManager _entityManager;
    private ModSuitMenu? _menu;

    public ModSuitBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
        _entityManager = IoCManager.Resolve<IEntityManager>();
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<ModSuitMenu>();
        _menu.SetEntity(Owner);

        _menu.ToggleModuleButtonPressed += SendModuleToggleMessage;
        _menu.PartToggleModulesButtonPressed += SendPartModulesToggleMessage;

        _menu.OpenCentered();
    }

    private void SendPartModulesToggleMessage(EntityUid uid)
    {
        var message = new TogglePartModulesUiMessage(_entityManager.GetNetEntity(uid));
        SendPredictedMessage(message);
    }

    private void SendModuleToggleMessage(EntityUid uid)
    {
        var message = new ToggleModuleUiMessage(_entityManager.GetNetEntity(uid));
        SendPredictedMessage(message);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not ModSuitBuiState msg)
            return;
        _menu?.UpdateState(msg);
    }
}
