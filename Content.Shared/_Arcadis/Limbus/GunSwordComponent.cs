using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Arcadis.Limbus;

/// <summary>
/// You darn should have unsheathed that sword if you really wanted to win!
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GunSwordComponent : Component
{
    [DataField, AutoNetworkedField]
    public float DamageAmplifier = 5f; // if i catch anyone increasing this i will ensure lei heng goes after you personally

    [DataField, AutoNetworkedField]
    public bool ManageKnockback = true;

    [DataField, AutoNetworkedField]
    public float KnockbackForce = 20f;

    [DataField, AutoNetworkedField]
    public int ShellsPerHit = 1;

    [AutoNetworkedField]
    public bool IsActive = true;
}
