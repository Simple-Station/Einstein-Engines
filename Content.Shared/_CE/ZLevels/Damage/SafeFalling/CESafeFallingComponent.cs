/*
 * This file is sublicensed under MIT License
 * https://github.com/space-wizards/space-station-14/blob/master/LICENSE.TXT
 */

using Robust.Shared.GameStates;

namespace Content.Shared._CE.ZLevels.Damage.SafeFalling;

/// <summary>
/// Reduces damage from falling on this entity
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CESafeFallingComponent : Component
{
    [DataField, AutoNetworkedField]
    public float DamageMultiplier = 0f;

    [DataField, AutoNetworkedField]
    public float StunMultiplier = 0f;
}
