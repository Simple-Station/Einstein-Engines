using Robust.Client.Graphics;
using Robust.Shared.GameStates;

namespace Content.Client.DeltaV.Harpy.Components;

[RegisterComponent]
public sealed partial class FlyingVisualsComponent : Component
{
    /// <summary>
    /// How long does the animation last
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public float Speed = 1f;

    /// <summary>
    /// How far it goes in any direction.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public float Distance = 1f;

    /// <summary>
    /// How much the limbs (if there are any) rotate.
    /// </summary>
    [ViewVariables(VVAccess.ReadWrite)]
    [DataField]
    public float Rotation = 1f;

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
