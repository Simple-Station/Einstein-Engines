using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Targeting;
using Content.Shared.Targeting.Events;
using Robust.Shared.Network;
using System.Linq;

namespace Content.Shared.Body.Systems;

public partial class SharedBodySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        foreach (var part in EntityManager.EntityQuery<BodyPartComponent>(false))
        {
            part.HealingTimer += frameTime;

            if (part.HealingTimer >= part.HealingTime)
            {
                part.HealingTimer = 0;
                if (part.Integrity < 100
                    && part.Integrity > 50
                    && part.ParentSlot is not null
                    && part.Body is not null
                    && !_mobState.IsDead(part.Body.Value))
                {
                    if (part.Integrity + part.SelfHealingAmount > 100)
                        part.Integrity = 100;
                    else
                        part.Integrity += part.SelfHealingAmount;
                }
            }

        }
    }

    /// <summary>
    /// Propagates damage to the specified parts of the entity.
    /// </summary>
    private void ApplyPartDamage(Entity<BodyPartComponent> partEnt, DamageSpecifier damage,
        BodyPartType targetType, TargetBodyPart targetPart, bool canSever, float partMultiplier)
    {
        var severingDamageTypes = new[] { "Slash", "Pierce", "Blunt" };
        var severed = false;
        var partIdSlot = GetParentPartAndSlotOrNull(partEnt)?.Slot;
        var originalIntegrity = partEnt.Comp.Integrity;
        foreach (var (damageType, damageValue) in damage.DamageDict)
        {
            if (damageValue.Float() == 0)
                continue;

            float modifier = GetDamageModifier(damageType);
            float partModifier = GetPartDamageModifier(targetType);
            partEnt.Comp.Integrity -= damageValue.Float() * modifier * partModifier * partMultiplier;
            if (canSever
                && severingDamageTypes.Contains(damageType)
                && partIdSlot is not null
                && partEnt.Comp.Integrity <= 0)
            {
                severed = true;
                break;
            }
        }

        if (partEnt.Comp.Integrity != originalIntegrity
            && TryComp<TargetingComponent>(partEnt.Comp.Body, out var targeting)
            && TryComp<MobStateComponent>(partEnt.Comp.Body, out var _) && partEnt.Comp.Body is not null)
        {
            var newIntegrity = GetIntegrityThreshold(partEnt.Comp.Integrity, severed);

            // We need to check if the part is dead to prevent the UI from showing dead parts as alive.
            if (targeting.BodyStatus[targetPart] != TargetIntegrity.Dead)
            {
                targeting.BodyStatus[targetPart] = newIntegrity;
                Dirty(partEnt.Comp.Body.Value, targeting);
            }

            // Revival events are handled by the server, so ends up being locked to a network event.
            if (_net.IsServer)
                RaiseNetworkEvent(new TargetIntegrityChangeEvent(GetNetEntity(partEnt.Comp.Body.Value)), partEnt.Comp.Body.Value);
        }

        // This will also prevent the torso from being removed.
        if (severed
            && partIdSlot is not null
            && partEnt.Comp.Body is not null)
        {
            DropPart(partEnt);
        }

        Dirty(partEnt, partEnt.Comp);
    }

    /// <summary>
    /// Gets the integrity of all body parts in the entity.
    /// </summary>
    public Dictionary<TargetBodyPart, TargetIntegrity> GetBodyPartStatus(EntityUid entityUid)
    {
        var result = new Dictionary<TargetBodyPart, TargetIntegrity>();

        if (!TryComp<BodyComponent>(entityUid, out var body))
            return result;

        foreach (TargetBodyPart part in Enum.GetValues(typeof(TargetBodyPart)))
        {
            result[part] = TargetIntegrity.Severed;
        }

        foreach (var partComponent in GetBodyChildren(entityUid, body))
        {
            var targetBodyPart = GetTargetBodyPart(partComponent.Component.PartType, partComponent.Component.Symmetry);

            if (targetBodyPart != null)
            {
                result[targetBodyPart.Value] = GetIntegrityThreshold(partComponent.Component.Integrity, false);
            }
        }

        return result;
    }

    /// <summary>
    /// Converts Enums from BodyPartType to their Targeting system equivalent.
    /// </summary>
    private TargetBodyPart? GetTargetBodyPart(BodyPartType type, BodyPartSymmetry symmetry)
    {
        return (type, symmetry) switch
        {
            (BodyPartType.Head, _) => TargetBodyPart.Head,
            (BodyPartType.Torso, _) => TargetBodyPart.Torso,
            (BodyPartType.Arm, BodyPartSymmetry.Left) => TargetBodyPart.LeftArm,
            (BodyPartType.Arm, BodyPartSymmetry.Right) => TargetBodyPart.RightArm,
            (BodyPartType.Leg, BodyPartSymmetry.Left) => TargetBodyPart.LeftLeg,
            (BodyPartType.Leg, BodyPartSymmetry.Right) => TargetBodyPart.RightLeg,
            _ => null
        };
    }

    /// <summary>
    /// Converts Enums from Targeting system to their BodyPartType equivalent.
    /// </summary>
    public (BodyPartType Type, BodyPartSymmetry Symmetry) ConvertTargetBodyPart(TargetBodyPart targetPart)
    {
        return targetPart switch
        {
            TargetBodyPart.Head => (BodyPartType.Head, BodyPartSymmetry.None),
            TargetBodyPart.Torso => (BodyPartType.Torso, BodyPartSymmetry.None),
            TargetBodyPart.LeftArm => (BodyPartType.Arm, BodyPartSymmetry.Left),
            TargetBodyPart.RightArm => (BodyPartType.Arm, BodyPartSymmetry.Right),
            TargetBodyPart.LeftLeg => (BodyPartType.Leg, BodyPartSymmetry.Left),
            TargetBodyPart.RightLeg => (BodyPartType.Leg, BodyPartSymmetry.Right),
            _ => (BodyPartType.Torso, BodyPartSymmetry.None)
        };

    }

    /// <summary>
    /// Fetches the damage multiplier for part integrity based on damage types.
    /// </summary>
    private float GetDamageModifier(string damageType)
    {
        return damageType switch
        {
            "Blunt" => 1.3f,
            "Slash" => 1.3f,
            "Pierce" => 1.3f,
            "Heat" => 1.0f,
            "Cold" => 1.0f,
            "Shock" => 0.8f,
            "Poison" => 0.8f,
            "Radiation" => 0.8f,
            "Cellular" => 0.8f,
            _ => 0.5f
        };
    }

    /// <summary>
    /// Fetches the damage multiplier for part integrity based on part types.
    /// </summary>
    private float GetPartDamageModifier(BodyPartType partType)
    {
        return partType switch
        {
            BodyPartType.Head => 1.0f,
            BodyPartType.Torso => 1.0f,
            BodyPartType.Arm => 1.0f,
            BodyPartType.Leg => 1.0f,
            _ => 0.5f
        };
    }

    /// <summary>
    /// Fetches the TargetIntegrity equivalent of the current integrity value for the body part.
    /// </summary>
    private TargetIntegrity GetIntegrityThreshold(float integrity, bool severed)
    {
        if (severed)
            return TargetIntegrity.Severed;
        else
            return integrity switch
            {
                <= 10.0f => TargetIntegrity.CriticallyWounded,
                <= 25.0f => TargetIntegrity.HeavilyWounded,
                <= 40.0f => TargetIntegrity.ModeratelyWounded,
                <= 60.0f => TargetIntegrity.SomewhatWounded,
                <= 80.0f => TargetIntegrity.LightlyWounded,
                _ => TargetIntegrity.Healthy
            };
    }


}