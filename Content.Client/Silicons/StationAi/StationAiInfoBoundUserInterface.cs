using Robust.Client.UserInterface;
using JetBrains.Annotations;
using Content.Shared.CrewManifest;
using Content.Shared.Silicons.StationAi;

namespace Content.Client.Silicons.StationAi;

[UsedImplicitly]
public sealed class StationAiInfoBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private StationAiInfo? _window;

    public StationAiInfoBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<StationAiInfo>();
        _window.CrewManifestButton.OnPressed += _ => SendMessage(new CrewManifestOpenUiMessage());
        _window.RoboticsControlButton.OnPressed += _ => SendMessage(new RoboticsControlOpenUiMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not StationAiInfoUpdateState updateState || _window == null)
            return;

        _window.UpdateState(updateState);
    }

}
