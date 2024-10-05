using Content.Shared.Body.Part;
using Content.Shared.Damage;
using Content.Shared.Targeting;
using System.Linq;

namespace Content.Shared.Body.Systems;

public partial class SharedBodySystem
{

    /// <summary>
    /// Propagates damage to the specified parts of the entity.
    /// </summary>
    private void ApplyPartDamage(Entity<BodyPartComponent> partEnt, DamageSpecifier damage, BodyPartType targetType)
    {
        var severingDamageTypes = new[] { "Slash", "Pierce", "Blunt" };
        var severed = false;
        var partIdSlot = GetParentPartAndSlotOrNull(partEnt)?.Slot;
        foreach (var (damageType, damageValue) in damage.DamageDict)
        {
            float modifier = GetDamageModifier(damageType);
            float partModifier = GetPartDamageModifier(targetType);
            partEnt.Comp.Integrity -= damageValue.Float() * modifier;
            if (severingDamageTypes.Contains(damageType) && partIdSlot is not null && partEnt.Comp.Integrity <= 0)
            {
                severed = true;
                break;
            }
        }

        // This will also prevent the torso from being removed.
        if (severed && partIdSlot is not null && partEnt.Comp.Body is not null)
        {
            DropPart(partEnt);
        }

        Dirty(partEnt, partEnt.Comp);
    }

    /// <summary>
    /// Converts Enums from Targeting system to their BodyPartType equivalent.
    /// </summary>
    public (BodyPartType Type, BodyPartSymmetry Symmetry) ConvertTargetBodyPart(TargetBodyPart targetPart)
    {
        switch (targetPart)
        {
            case TargetBodyPart.Head:
                return (BodyPartType.Head, BodyPartSymmetry.None);
            case TargetBodyPart.Torso:
                return (BodyPartType.Torso, BodyPartSymmetry.None);
            case TargetBodyPart.LeftArm:
                return (BodyPartType.Arm, BodyPartSymmetry.Left);
            case TargetBodyPart.RightArm:
                return (BodyPartType.Arm, BodyPartSymmetry.Right);
            case TargetBodyPart.LeftLeg:
                return (BodyPartType.Leg, BodyPartSymmetry.Left);
            case TargetBodyPart.RightLeg:
                return (BodyPartType.Leg, BodyPartSymmetry.Right);
            default:
                return (BodyPartType.Torso, BodyPartSymmetry.None);
        }
    }

    /// <summary>
    /// Fetches the damage multiplier for part integrity based on damage types.
    /// </summary>
    private float GetDamageModifier(string damageType)
    {
        switch (damageType)
        {
            case "Blunt":
                return 1.3f;
            case "Slash":
                return 1.3f;
            case "Pierce":
                return 1.3f;
            case "Heat":
                return 1.0f;
            case "Cold":
                return 1.0f;
            case "Shock":
                return 0.8f;
            case "Poison":
                return 0.8f;
            case "Radiation":
                return 0.8f;
            case "Cellular":
                return 0.8f;
            default:
                return 0.5f;
        }
    }

    /// <summary>
    /// Fetches the damage multiplier for part integrity based on part types.
    /// </summary>
    private float GetPartDamageModifier(BodyPartType partType)
    {
        switch (partType)
        {
            case BodyPartType.Head:
                return 1.0f;
            case BodyPartType.Torso:
                return 1.0f;
            case BodyPartType.Arm:
                return 1.0f;
            case BodyPartType.Leg:
                return 1.0f;
            default:
                return 0.5f;
        }
    }

}
