using Content.Shared.Damage;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._Crescent.DegradeableArmor;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DegradeableArmorComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float armorMaxHealth = 500f;
    /// <summary>
    /// Leave at 0 for it to get automatically set to armor max health(unless you want it hardset at lower or higher)
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public float armorHealth;

    /// <summary>
    /// percentage of blocked damage that gets converted into stamina damage
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public Dictionary<string, float> staminaConversions = new Dictionary<string, float>()
    {
        {"Blunt", 0.25f},
        {"Slash", 0.15f},
        {"Piercing", 0.15f},
        {"Heat", 0f},
        {"Caustic", 0f},
        {"Radiation", 0f}
    };
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public Dictionary<string, float> armorDamageCoefficients = new Dictionary<string, float>()
    {
        {"Blunt", 1.4f},
        {"Slash", 1.7f},
        {"Piercing", 1f},
        {"Heat", 1f},
        {"Caustic", 10f},
        {"Radiation", 0.1f}
    };
    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public Dictionary<string, float> maxBlockCoefficients = new Dictionary<string, float>()
    {
        {"Blunt", 0.8f},
        {"Slash", 0.8f},
        {"Piercing", 0.8f},
        {"Heat", 0.8f},
        {"Caustic", 0.8f},
        {"Radiation", 1f}
    };

    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public ArmorDegradation armorType = ArmorDegradation.Plastic;

    [DataField, ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public ArmorRepairMaterial armorRepair = ArmorRepairMaterial.PlasteelPlate;


    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public DamageModifierSet initialModifiers = default!;

    /// <summary>
    ///  This is shitty but there is no wizden implementation for getting the wearer of a piece of clothing
    /// </summary>
    ///
    [ViewVariables(VVAccess.ReadWrite)] 
    public EntityUid wearer = EntityUid.Invalid;

}
[Serializable, NetSerializable]
public enum ArmorDegradation
{
    Ceramic = 1, // blocks damage but decay is exponential to the damage. 
    Metallic = 1<<1, // Linear damaage , linear scaling of protection
    Plastic = 1<<2, // Complicated
}

