using Content.Server.Shuttles.Systems;
using Content.Shared.Shuttles.Components;

namespace Content.Server.Shuttles.Components;

[RegisterComponent, Access(typeof(ShuttleSystem))]
public sealed partial class IFFConsoleComponent : Component
{
    /// <summary>
    /// Flags that this console is allowed to set.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField("allowedFlags")]
    public IFFFlags AllowedFlags = IFFFlags.HideLabel;

    [ViewVariables(VVAccess.ReadWrite), DataField("allowColorChange")]
    public bool AllowColorChange = false;

    [ViewVariables(VVAccess.ReadWrite)]
    public EntityUid? originalGrid = null;

    // Hullrot edit - SCPR 2025
    [DataField("heatCapacity"),ViewVariables(VVAccess.ReadWrite)]
    public float HeatCapacity = 300f;

    [ViewVariables(VVAccess.ReadWrite)]
    public float CurrentHeat = 0f;

    /// <summary>
    /// Heat generation for every 1 seconds
    /// </summary>
    [DataField("heatGeneration"), ViewVariables(VVAccess.ReadWrite)]
    public float HeatGeneration = 10f;

    /// <summary>
    /// Heat dissipation for every 1 seconds , only active when not cloaked.
    /// </summary>
    ///
    [DataField("heatDissipation"), ViewVariables(VVAccess.ReadWrite)]
    public float HeatDissipation = 1f;

    public bool active = false;
    // end
}
