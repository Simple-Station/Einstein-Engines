using Robust.Shared.GameStates;

namespace Content.Shared.Crescent.Radar;

/// <summary>
/// Handles what a turret should look like on radar.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TurretIFFComponent : Component
{
    /// <summary>
    /// Default color to use for IFF if no component is found.
    /// </summary>
    public static readonly Color DefaultColor = Color.DarkOrange;
    public static readonly Color DefaultSelfColor = Color.Yellow;
    public static readonly Color DefaultControlledColor = Color.YellowGreen;
}
