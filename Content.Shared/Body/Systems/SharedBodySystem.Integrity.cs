using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;
using Content.Shared.Medical.Surgery.Steps.Parts;
using Content.Shared.Mobs.Components;
using Content.Shared.Standing;
using Content.Shared.Targeting;
using Content.Shared.Targeting.Events;
using Robust.Shared.Network;
using Robust.Shared.Random;
using System.Linq;

namespace Content.Shared.Body.Systems;

public partial class SharedBodySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    private readonly string[] _severingDamageTypes = { "Slash", "Pierce", "Blunt" };
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<BodyPartComponent>();
        while (query.MoveNext(out var ent, out var part))
        {
            if (!_timing.IsFirstTimePredicted
                || !HasComp<TargetingComponent>(ent))
                continue;

            part.HealingTimer += frameTime;

            if (part.HealingTimer >= part.HealingTime)
            {
                part.HealingTimer = 0;
                if (part.Body is not null
                    && part.Integrity > 50
                    && part.Integrity < 100
                    && !_mobState.IsDead(part.Body.Value))
                {
                    var healing = part.SelfHealingAmount;
                    if (healing + part.Integrity > 100)
                        healing = part.Integrity - 100;

                    TryChangeIntegrity((ent, part), healing,
                        false, GetTargetBodyPart(part.PartType, part.Symmetry), out _);
                }
            }

        }
        query.Dispose();
    }

    /// <summary>
    /// Propagates damage to the specified parts of the entity.
    /// </summary>
    private void ApplyPartDamage(Entity<BodyPartComponent> partEnt, DamageSpecifier damage,
        BodyPartType targetType, TargetBodyPart targetPart, bool canSever, float partMultiplier)
    {
        if (partEnt.Comp.Body is null)
            return;

        foreach (var (damageType, damageValue) in damage.DamageDict)
        {
            if (damageValue.Float() == 0
                || TryEvadeDamage(partEnt.Comp.Body.Value, GetEvadeChance(targetType)))
                continue;

            float modifier = GetDamageModifier(damageType);
            float partModifier = GetPartDamageModifier(targetType);
            float integrityDamage = damageValue.Float() * modifier * partModifier * partMultiplier;
            TryChangeIntegrity(partEnt, integrityDamage, canSever && _severingDamageTypes.Contains(damageType),
                targetPart, out var severed);

            if (severed)
                break;
        }
    }

    public void TryChangeIntegrity(Entity<BodyPartComponent> partEnt, float integrity, bool canSever,
        TargetBodyPart? targetPart, out bool severed)
    {
        severed = false;
        if (!HasComp<TargetingComponent>(partEnt.Comp.Body) || !_timing.IsFirstTimePredicted)
            return;

        var partIdSlot = GetParentPartAndSlotOrNull(partEnt)?.Slot;
        var originalIntegrity = partEnt.Comp.Integrity;
        partEnt.Comp.Integrity = Math.Min(100, partEnt.Comp.Integrity - integrity);
        if (canSever
            && !HasComp<BodyPartReattachedComponent>(partEnt)
            && !partEnt.Comp.Enabled
            && partEnt.Comp.Integrity <= 0
            && partIdSlot is not null)
            severed = true;

        // This will also prevent the torso from being removed.
        if (partEnt.Comp.Enabled
            && partEnt.Comp.Integrity <= 15.0f)
        {
            var ev = new BodyPartEnableChangedEvent(false);
            RaiseLocalEvent(partEnt, ref ev);
        }
        else if (!partEnt.Comp.Enabled
            && partEnt.Comp.Integrity >= 80.0f)
        {
            var ev = new BodyPartEnableChangedEvent(true);
            RaiseLocalEvent(partEnt, ref ev);
        }

        if (partEnt.Comp.Integrity != originalIntegrity
            && TryComp<TargetingComponent>(partEnt.Comp.Body, out var targeting)
            && TryComp<MobStateComponent>(partEnt.Comp.Body, out var _))
        {
            var newIntegrity = GetIntegrityThreshold(partEnt.Comp.Integrity, severed, partEnt.Comp.Enabled);
            // We need to check if the part is dead to prevent the UI from showing dead parts as alive.
            if (targetPart is not null && targeting.BodyStatus[targetPart.Value] != TargetIntegrity.Dead)
            {
                targeting.BodyStatus[targetPart.Value] = newIntegrity;
                Dirty(partEnt.Comp.Body.Value, targeting);
            }

            // Revival events are handled by the server, so ends up being locked to a network event.
            if (_net.IsServer)
                RaiseNetworkEvent(new TargetIntegrityChangeEvent(GetNetEntity(partEnt.Comp.Body.Value)), partEnt.Comp.Body.Value);
        }

        if (severed && partIdSlot is not null)
            DropPart(partEnt);

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
                result[targetBodyPart.Value] = GetIntegrityThreshold(partComponent.Component.Integrity, false, partComponent.Component.Enabled);
            }
        }

        return result;
    }

    /// <summary>
    /// Converts Enums from BodyPartType to their Targeting system equivalent.
    /// </summary>
    public TargetBodyPart? GetTargetBodyPart(BodyPartType type, BodyPartSymmetry symmetry)
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
    public float GetDamageModifier(string damageType)
    {
        return damageType switch
        {
            "Blunt" => 0.8f,
            "Slash" => 1.2f,
            "Pierce" => 0.5f,
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
    public float GetPartDamageModifier(BodyPartType partType)
    {
        return partType switch
        {
            BodyPartType.Head => 0.5f, // 50% damage, necks are hard to cut
            BodyPartType.Torso => 1.0f, // 100% damage
            BodyPartType.Arm => 0.7f, // 70% damage
            BodyPartType.Leg => 0.7f, // 70% damage
            _ => 0.5f
        };
    }

    /// <summary>
    /// Fetches the TargetIntegrity equivalent of the current integrity value for the body part.
    /// </summary>
    public TargetIntegrity GetIntegrityThreshold(float integrity, bool severed, bool enabled)
    {
        if (severed)
            return TargetIntegrity.Severed;
        else if (!enabled)
            return TargetIntegrity.Disabled;
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

    /// <summary>
    /// Fetches the chance to evade integrity damage for a body part.
    /// Used when the entity is not dead, laying down, or incapacitated.
    /// </summary>
    public float GetEvadeChance(BodyPartType partType)
    {
        return partType switch
        {
            BodyPartType.Head => 0.70f,  // 70% chance to evade
            BodyPartType.Arm => 0.20f,   // 20% chance to evade
            BodyPartType.Leg => 0.20f,   // 20% chance to evade
            BodyPartType.Torso => 0f, // 0% chance to evade
            _ => 0f
        };
    }

    public bool CanEvadeDamage(EntityUid uid)
    {
        if (!TryComp<MobStateComponent>(uid, out var mobState)
            || !TryComp<StandingStateComponent>(uid, out var standingState)
            || _mobState.IsCritical(uid, mobState)
            || _mobState.IsDead(uid, mobState)
            || standingState.CurrentState == StandingState.Lying)
            return false;

        return true;
    }

    public bool TryEvadeDamage(EntityUid uid, float evadeChance)
    {
        if (!CanEvadeDamage(uid))
            return false;

        return _random.NextFloat() < evadeChance;
    }

}