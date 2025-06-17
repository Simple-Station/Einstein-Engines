namespace Content.Shared.Psionics.Components;

[RegisterComponent]
public sealed partial class PotentiaContainerComponent : Component
{
    /// <summary>
    ///     Potentia is a shared resource that can be used by psionic features, which represents available "Psychic Potential".
    /// </summary>
    [DataField]
    public float Potentia;
}
