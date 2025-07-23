using Robust.Shared.GameStates;
using Robust.Shared.Maths;
using Robust.Shared.Serialization;

namespace Content.Shared.Crescent.Radar
{
    /// <summary>
    /// Handles what a turret should look like on radar.
    /// </summary>
    [RegisterComponent, NetworkedComponent]
    public sealed partial class TurretIFFComponent : Component
    {
        // DarkOrange RGB: (255, 140, 0)
        public static readonly Color DefaultColor = new Color(255, 140, 0, 255);

        // Yellow RGB: (255, 255, 0)
        public static readonly Color DefaultControlledColor = new Color(255, 255, 0, 255);

        // YellowGreen RGB: (154, 205, 50)
        public static readonly Color DefaultSelfColor = new Color(154, 205, 50, 255);
    }
}
