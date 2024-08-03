using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Showers
{
    /// <summary>
    /// showers that can be enabled
    /// </summary>
    [RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
    public sealed partial class ShowerComponent : Component
    {
        /// <summary>
        /// Toggles shower.
        /// </summary>
        [DataField, AutoNetworkedField]
        public bool ToggleShower;

    }

    [Serializable, NetSerializable]
    public enum ShowerVisuals : byte
    {
        ShowerVisualState,
    }

    [Serializable, NetSerializable]
    public enum ShowerVisualState : byte
    {
        Off,
        On
    }
}

