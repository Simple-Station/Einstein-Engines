using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared.Gravity
{
    [RegisterComponent]
    [AutoGenerateComponentState]
    [NetworkedComponent]
    public sealed partial class GravityComponent : Component
    {
        [DataField, AutoNetworkedField]
        public SoundSpecifier GravityShakeSound { get; set; } = new SoundPathSpecifier("/Audio/Effects/alert.ogg");

        [DataField, AutoNetworkedField]
        public bool Enabled;

        /// <summary>
        ///     Default acceleration due to gravity in meters per second squared, equivalent to Earth's gravity at sea level.
        /// </summary>
        [DataField]
        public double Acceleration = 9.80665;

        /// <summary>
        /// Inherent gravity ensures GravitySystem won't change Enabled according to the gravity generators attached to this entity.
        /// </summary>
        [DataField, AutoNetworkedField]
        public bool Inherent;
    }
}
