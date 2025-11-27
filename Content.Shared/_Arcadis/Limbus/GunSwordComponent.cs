using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Arcadis.Limbus;

/// <summary>
/// But... Yoshihide...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GunSwordComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Container = "ballistic-ammo";

    [DataField, AutoNetworkedField]
    public int ShellsPerHit = 1;

    [AutoNetworkedField]
    public bool IsActive = true;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? SoundEject = new SoundPathSpecifier("/Audio/Weapons/Guns/MagOut/revolver_magout.ogg");
}

/// <summary>
/// You darn should have unsheathed that sword if you really wanted to win!
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GunSwordAmmoComponent : Component
{
    [DataField, AutoNetworkedField]
    public float DamageAmplifier = 3f; // if i catch anyone increasing this, 2 week repoban. this is a threat

    [DataField, AutoNetworkedField]
    public float KnockbackForce = 2f;

    [DataField, AutoNetworkedField]
    public float ReflectChanceIncrease = 5;
}
