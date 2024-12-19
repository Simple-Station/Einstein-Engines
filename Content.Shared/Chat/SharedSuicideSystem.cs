using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Mobs.Components;
using Robust.Shared.Prototypes;
using System.Linq;

namespace Content.Shared.Chat;

public sealed class SharedSuicideSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly ILogManager _logManager = default!;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = _logManager.GetSawmill("suicide");
    }

    /// <summary>
    /// Applies lethal damage spread out across the damage types given.
    /// </summary>
    public void ApplyLethalDamage(Entity<DamageableComponent> target, DamageSpecifier damageSpecifier)
    {
        // Create a new damageSpecifier so that we don't make alterations to the original DamageSpecifier
        // Failing  to do this will permanently change a weapon's damage making it insta-kill people
        var appliedDamageSpecifier = new DamageSpecifier(damageSpecifier);
        var damagesStr = "";

        foreach (var damage in appliedDamageSpecifier.DamageDict)
            damagesStr += damage.Key + ": " + damage.Value + "\n";

        _sawmill.Info($"Applying lethal damage (specifier) 1: \nnum damage types: {appliedDamageSpecifier.DamageDict.Count}\ndamages:\n{damagesStr}");
        if (!TryComp<MobThresholdsComponent>(target, out var mobThresholds))
            return;

        _sawmill.Info("Applying lethal damage (specifier) 2");

        // Mob thresholds are sorted from alive -> crit -> dead,
        // grabbing the last key will give us how much damage is needed to kill a target from zero
        // The exact lethal damage amount is adjusted based on their current damage taken
        var lethalAmountOfDamage = mobThresholds.Thresholds.Keys.Last() - target.Comp.TotalDamage;
        var totalDamage = appliedDamageSpecifier.GetTotal();
        _sawmill.Info("Applying lethal damage (specifier) 3");

        // Removing structural because it causes issues against entities that cannot take structural damage,
        // then getting the total to use in calculations for spreading out damage.
        appliedDamageSpecifier.DamageDict.Remove("Structural");

        // Split the total amount of damage needed to kill the target by every damage type in the DamageSpecifier
        foreach (var (key, value) in appliedDamageSpecifier.DamageDict)
            appliedDamageSpecifier.DamageDict[key] = Math.Ceiling((double) (value * lethalAmountOfDamage / totalDamage));

        _sawmill.Info("Applying lethal damage (specifier): " + appliedDamageSpecifier.GetTotal());

        _damageableSystem.TryChangeDamage(target, appliedDamageSpecifier, true, origin: target);
    }

    /// <summary>
    /// Applies lethal damage in a single type, specified by a single damage type.
    /// </summary>
    public void ApplyLethalDamage(Entity<DamageableComponent> target, ProtoId<DamageTypePrototype>? damageType)
    {
        _sawmill.Info("Applying lethal damage (singular) 1");
        if (!TryComp<MobThresholdsComponent>(target, out var mobThresholds))
            return;

        _sawmill.Info("Applying lethal damage (singular) 2");

        // Mob thresholds are sorted from alive -> crit -> dead,
        // grabbing the last key will give us how much damage is needed to kill a target from zero
        // The exact lethal damage amount is adjusted based on their current damage taken
        var lethalAmountOfDamage = mobThresholds.Thresholds.Keys.Last() - target.Comp.TotalDamage;
        _sawmill.Info("Applying lethal damage (singular): " + lethalAmountOfDamage);

        // We don't want structural damage for the same reasons listed above
        if (!_prototypeManager.TryIndex(damageType, out var damagePrototype) || damagePrototype.ID == "Structural")
        {
            Log.Error($"{nameof(SharedSuicideSystem)} could not find the damage type prototype associated with {damageType}. Falling back to Blunt");
            damagePrototype = _prototypeManager.Index<DamageTypePrototype>("Blunt");
        }

        var damage = new DamageSpecifier(damagePrototype, lethalAmountOfDamage);
        _sawmill.Info("Applying lethal damage (singular): " + damage.GetTotal());
        _damageableSystem.TryChangeDamage(target, damage, true, origin: target);
    }
}
