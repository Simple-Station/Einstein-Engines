namespace Content.Shared._Goobstation.Fishing.Components;

/// <summary>
/// The fish itself!
/// </summary>
[RegisterComponent]
public sealed partial class FishComponent : Component
{
    public const float DefaultDifficulty = 0.021f;

    [DataField("difficulty")]
    public float FishDifficulty = DefaultDifficulty;
}
