#region

using Content.Shared.Power.Generator;
using JetBrains.Annotations;

#endregion


namespace Content.Client.Power.Generator;


[UsedImplicitly]
public sealed class PortableGeneratorBoundUserInterface : BoundUserInterface
{
    private GeneratorWindow? _window;

    public PortableGeneratorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey) { }

    protected override void Open()
    {
        base.Open();
        _window = new(this, Owner);

        _window.OpenCenteredLeft();
        _window.OnClose += Close;
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not PortableGeneratorComponentBuiState msg)
            return;

        _window?.Update(msg);
    }

    protected override void Dispose(bool disposing) => _window?.Dispose();

    public void SetTargetPower(int target) => SendMessage(new PortableGeneratorSetTargetPowerMessage(target));

    public void Start() => SendMessage(new PortableGeneratorStartMessage());

    public void Stop() => SendMessage(new PortableGeneratorStopMessage());

    public void SwitchOutput() => SendMessage(new PortableGeneratorSwitchOutputMessage());

    public void EjectFuel() => SendMessage(new PortableGeneratorEjectFuelMessage());
}
