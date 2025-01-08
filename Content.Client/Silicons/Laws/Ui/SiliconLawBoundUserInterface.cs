using Content.Shared.Silicons.Laws.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client.Silicons.Laws.Ui;

[UsedImplicitly]
public sealed class SiliconLawBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private SiliconLawMenu? _menu;
    private EntityUid _owner;

    public SiliconLawBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        _owner = owner;
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<SiliconLawMenu>();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not SiliconLawBuiState msg)
            return;

        _menu?.Update(_owner, msg);
    }
}
