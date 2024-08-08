namespace Content.Server.Traits.Assorted;

/// <summary>
///     This is used for the Blood Deficiency trait.
/// </summary>
[RegisterComponent]
public sealed partial class BloodDeficiencyComponent : Component
{
    // <summary>
    //     How much reagent of blood should be removed in each update interval?
    // </summary>
    [DataField(required: true)]
    public float BloodLossAmount;
}
