using Robust.Shared.GameStates;

namespace Content.Shared._Shitmed.Body.Organ;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class HeartComponent : Component
{
    /// <summary>
    ///     The base capacity of the heart.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Capacity;

    /// <summary>
    ///     The current capacity of the heart.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float CurrentCapacity;
}
