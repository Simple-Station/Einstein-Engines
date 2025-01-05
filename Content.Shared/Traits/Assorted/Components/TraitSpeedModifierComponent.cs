using Robust.Shared.GameStates;

namespace Content.Shared.Traits.Assorted.Components;

/// <summary>
///  This component is used for traits that modify movement speed.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TraitSpeedModifierComponent : Component
{
    [DataField, AutoNetworkedField]
    public float WalkModifier = 1.0f;

    [DataField, AutoNetworkedField]
    public float SprintModifier = 1.0f;

    // <summary>
    //   Multiplied with the required trigger speed for step triggers that this entity collides with.
    // </summary>
    [DataField, AutoNetworkedField]
    public float RequiredTriggeredSpeedModifier = 1.0f;
}
