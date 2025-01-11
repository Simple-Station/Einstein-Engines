using Content.Shared.Movement.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Movement.Components;

// <summary>
//   This is used to modify how much an entity is affected by speed modifier contacts.
// </summary>
[NetworkedComponent, RegisterComponent]
[AutoGenerateComponentState]
[Access(typeof(SpeedModifierContactsSystem))]
public sealed partial class SpeedModifiedByContactModifierComponent : Component
{
    // <summary>
    //   Numbers greater than 1 amplify the walk speed modifier, and lower numbers lessen the effect.
    // </summary>
    [DataField, AutoNetworkedField]
    public float WalkModifierEffectiveness = 1.0f;

    // <summary>
    //   Numbers greater than 1 amplify the sprint speed modifier, and lower numbers lessen the effect.
    // </summary>
    [DataField, AutoNetworkedField]
    public float SprintModifierEffectiveness = 1.0f;

    // <summary>
    //   The minimum walk speed multiplier.
    // </summary>
    [DataField, AutoNetworkedField]
    public float MinWalkMultiplier = 0.1f;

    // <summary>
    //   The minimum sprint speed multiplier.
    // </summary>
    [DataField, AutoNetworkedField]
    public float MinSprintMultiplier = 0.1f;
}
