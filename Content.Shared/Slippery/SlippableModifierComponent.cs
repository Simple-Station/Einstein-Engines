using Robust.Shared.GameStates;
namespace Content.Shared.Slippery;

/// <summary>
///   Modifies the duration of slip paralysis on an entity.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SlippableModifierComponent : Component
{
    /// <summary>
    ///   What to multiply the paralyze time by.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ParalyzeTimeMultiplier = 1f;
}
