using System.Numerics;
using Robust.Shared.GameStates;

namespace Content.Shared.DeltaV.Harpy.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class FlyingVisualsComponent : Component
{
    /// <summary>
    /// How long does the animation last
    /// </summary>
    [DataField]
    public float AnimationTime = 2f;

    /// <summary>
    /// How far it goes in any direction.
    /// </summary>
    [DataField]
    public float Offset = 0.2f;

    /// <summary>
    /// How much the limbs (if there are any) rotate.
    /// </summary>
    [DataField]
    public float Rotation = 0.5f;

    [DataField]
    public string AnimationKey = default;
}
