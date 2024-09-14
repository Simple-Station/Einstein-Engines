using Robust.Client.Graphics;
using Robust.Shared.GameStates;

namespace Content.Client.Flight.Components;

[RegisterComponent]
public sealed partial class FlightVisualsComponent : Component
{
    /// <summary>
    /// How long does the animation last
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public float Speed;

    /// <summary>
    /// How far it goes in any direction.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public float Multiplier;

    /// <summary>
    /// How much the limbs (if there are any) rotate.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public float Offset;

    /// <summary>
    /// Are we animating layers or the entire sprite?
    /// </summary>
    public bool AnimateLayer = false;
    public int? TargetLayer;

    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public string AnimationKey = "default";

    [ViewVariables(VVAccess.ReadWrite)]
    public ShaderInstance Shader = default!;


}
