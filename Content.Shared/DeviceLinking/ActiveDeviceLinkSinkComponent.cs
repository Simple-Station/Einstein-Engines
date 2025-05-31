using Robust.Shared.GameStates;

namespace Content.Shared.DeviceLinking;

[RegisterComponent, NetworkedComponent]
public sealed partial class ActiveDeviceLinkSinkComponent : Component
{
    /// <summary>
    /// Counts the amount of times a sink has been invoked for severing the link if this counter gets to high
    /// The counter is counted down by one every tick if it's higher than 0
    /// This is for preventing infinite loops
    /// </summary>
    [DataField]
    public int InvokeCounter;
}
