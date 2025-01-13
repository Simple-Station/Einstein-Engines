using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Strip.Components
{

    /// <summary>
    /// This is used for marking an entity (mainly one with ItemComponent) as a hidden item in the Strip Menu.
    /// </summary>
    [RegisterComponent, NetworkedComponent]
    public sealed partial class StripMenuHiddenComponent : Component;

}
