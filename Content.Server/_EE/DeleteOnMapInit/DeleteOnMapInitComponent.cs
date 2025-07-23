namespace Content.Server._EE.DeleteOnMapInit;

/// <summary>
///     An entity with this component will immediately be deleted upon spawning.
///     Used for dummy limbs/organs to make body prototypes with missing limbs that can be re-added.
/// </summary>
[RegisterComponent]
public sealed partial class DeleteOnMapInitComponent : Component
{
}
