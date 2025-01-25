namespace Content.Client.Light.Components;

/// <summary>
/// Fades out the <see cref="SharedPointLightComponent"/> attached to this entity.
/// </summary>
[RegisterComponent]
public sealed partial class LightFadeComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite), DataField("duration")]
    public float Duration = 0.5f;

    // <summary>
    //   The duration of the fade-in effect before starting the fade out effect.
    // </summary>
    [ViewVariables(VVAccess.ReadWrite), DataField]
    public float RampUpDuration = 0f;
}
