/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Robust.Shared.GameStates;

namespace Content.Shared._CE.ZLevels.Damage.SoftPaws;

/// <summary>
/// Reduces fall damage and removes stun if the fall speed does not exceed a certain limit.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CESoftPawsComponent : Component
{
    /// <summary>
    /// The fall speed must be less than this for damage reduction and stun to start working.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float MaxSpeedLimit = 1f;

    [DataField, AutoNetworkedField]
    public float DamageMultiplier = 0f;

    [DataField, AutoNetworkedField]
    public float StunMultiplier = 0f;

    [DataField, AutoNetworkedField]
    public float DamageHardFallMultiplier = 0.5f;

    [DataField, AutoNetworkedField]
    public float StunHardFallMultiplier = 0.5f;
}
