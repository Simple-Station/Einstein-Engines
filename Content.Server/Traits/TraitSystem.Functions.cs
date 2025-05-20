using Content.Shared.FixedPoint;
using Content.Shared.Traits;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Content.Shared.Implants;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;
using Content.Shared.Actions;
using Content.Server.Abilities.Psionics;
using Content.Shared.Psionics;
using Content.Server.Language;
using Content.Shared.Mood;
using Content.Shared.Traits.Assorted.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Mobs;
using Content.Shared.NPC.Systems;
using Content.Shared.Weapons.Melee;
using Robust.Shared.Audio;
using Content.Shared.Tag;
using Content.Shared.Body.Part;
using Content.Server.Body.Systems;
using Content.Shared.Body.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using System.Linq;
using Robust.Shared.Utility;
using Robust.Shared.GameStates;

namespace Content.Server.Traits;

/// Used for traits that add a Component upon spawning in, overwriting the pre-existing component if it already exists.
[UsedImplicitly]
public sealed partial class TraitReplaceComponent : TraitFunction
{
    [DataField, AlwaysPushInheritance]
    public ComponentRegistry Components { get; private set; } = new();

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        foreach (var (_, data) in Components)
        {
            var comp = (Component) serializationManager.CreateCopy(data.Component, notNullableOverride: true);
            comp.Owner = uid;
            entityManager.AddComponent(uid, comp, true);
            if (!comp.GetType().HasCustomAttribute<NetworkedComponentAttribute>())
                continue;

            entityManager.Dirty(uid, comp);
        }
    }
}

/// <summary>
///     Used for traits that add a Component upon spawning in.
///     This will do nothing if the Component already exists.
/// </summary>
[UsedImplicitly]
public sealed partial class TraitAddComponent : TraitFunction
{
    [DataField, AlwaysPushInheritance]
    public ComponentRegistry Components { get; private set; } = new();

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        foreach (var entry in Components.Values)
        {
            if (entityManager.HasComponent(uid, entry.Component.GetType()))
                continue;

            var comp = (Component) serializationManager.CreateCopy(entry.Component, notNullableOverride: true);
            comp.Owner = uid;
            entityManager.AddComponent(uid, comp);

            if (!comp.GetType().HasCustomAttribute<NetworkedComponentAttribute>())
                continue;
            entityManager.Dirty(uid, comp);
        }
    }
}

/// Used for traits that remove a component upon a player spawning in.
[UsedImplicitly]
public sealed partial class TraitRemoveComponent : TraitFunction
{
    [DataField, AlwaysPushInheritance]
    public ComponentRegistry Components { get; private set; } = new();

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        foreach (var (name, _) in Components)
            entityManager.RemoveComponentDeferred(uid, factory.GetComponent(name).GetType());
    }
}

/// Used for traits that add an action upon a player spawning in.
[UsedImplicitly]
public sealed partial class TraitAddActions : TraitFunction
{
    [DataField, AlwaysPushInheritance]
    public List<EntProtoId> Actions { get; private set; } = new();

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        var actionSystem = entityManager.System<SharedActionsSystem>();

        foreach (var id in Actions)
        {
            EntityUid? actionId = null;
            if (actionSystem.AddAction(uid, ref actionId, id))
                actionSystem.StartUseDelay(actionId);
        }
    }
}

/// Used for traits that add an Implant upon spawning in.
[UsedImplicitly]
public sealed partial class TraitAddImplant : TraitFunction
{
    [DataField(customTypeSerializer: typeof(PrototypeIdHashSetSerializer<EntityPrototype>))]
    [AlwaysPushInheritance]
    public HashSet<string> Implants { get; private set; } = new();

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        var implantSystem = entityManager.System<SharedSubdermalImplantSystem>();
        implantSystem.AddImplants(uid, Implants);
    }
}

/// <summary>
///     If a trait includes any Psionic Powers, this enters the powers into PsionicSystem to be initialized.
///     If the lack of logic here seems startling, it's okay. All of the logic necessary for adding Psionics is handled by InitializePsionicPower.
/// </summary>
[UsedImplicitly]
public sealed partial class TraitAddPsionics : TraitFunction
{
    [DataField, AlwaysPushInheritance]
    public List<ProtoId<PsionicPowerPrototype>> PsionicPowers { get; private set; } = new();

    [DataField, AlwaysPushInheritance]
    public bool PlayFeedback;

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        var prototype = IoCManager.Resolve<IPrototypeManager>();
        var psionic = entityManager.System<PsionicAbilitiesSystem>();

        foreach (var powerProto in PsionicPowers)
            if (prototype.TryIndex(powerProto, out var psionicPower))
                psionic.InitializePsionicPower(uid, psionicPower, PlayFeedback);
    }
}

/// <summary>
///     This isn't actually used for any traits, surprise, other systems can use these functions!
///     This is used by Items of Power to remove a psionic power when unequipped.
/// </summary>
[UsedImplicitly]
public sealed partial class TraitRemovePsionics : TraitFunction
{
    [DataField, AlwaysPushInheritance]
    public List<ProtoId<PsionicPowerPrototype>> PsionicPowers { get; private set; } = new();

    [DataField, AlwaysPushInheritance]
    public bool Forced = true;

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        var prototype = IoCManager.Resolve<IPrototypeManager>();
        var psionic = entityManager.System<PsionicAbilitiesSystem>();

        foreach (var powerProto in PsionicPowers)
            if (prototype.TryIndex(powerProto, out var psionicPower))
                psionic.RemovePsionicPower(uid, psionicPower, Forced);
    }
}

/// Handles all modification of Known Languages. Removes languages before adding them.
[UsedImplicitly]
public sealed partial class TraitModifyLanguages : TraitFunction
{
    /// The list of all Spoken Languages that this trait adds.
    [DataField, AlwaysPushInheritance]
    public List<string>? LanguagesSpoken { get; private set; } = default!;

    /// The list of all Understood Languages that this trait adds.
    [DataField, AlwaysPushInheritance]
    public List<string>? LanguagesUnderstood { get; private set; } = default!;

    /// The list of all Spoken Languages that this trait removes.
    [DataField, AlwaysPushInheritance]
    public List<string>? RemoveLanguagesSpoken { get; private set; } = default!;

    /// The list of all Understood Languages that this trait removes.
    [DataField, AlwaysPushInheritance]
    public List<string>? RemoveLanguagesUnderstood { get; private set; } = default!;

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        var language = entityManager.System<LanguageSystem>();

        if (RemoveLanguagesSpoken is not null)
            foreach (var lang in RemoveLanguagesSpoken)
                language.RemoveLanguage(uid, lang, true, false);

        if (RemoveLanguagesUnderstood is not null)
            foreach (var lang in RemoveLanguagesUnderstood)
                language.RemoveLanguage(uid, lang, false, true);

        if (LanguagesSpoken is not null)
            foreach (var lang in LanguagesSpoken)
                language.AddLanguage(uid, lang, true, false);

        if (LanguagesUnderstood is not null)
            foreach (var lang in LanguagesUnderstood)
                language.AddLanguage(uid, lang, false, true);
    }
}

/// Handles adding Moodlets to a player character upon spawning in. Typically used for permanent moodlets or drug addictions.
[UsedImplicitly]
public sealed partial class TraitAddMoodlets : TraitFunction
{
    /// The list of all Moodlets that this trait adds.
    [DataField, AlwaysPushInheritance]
    public List<ProtoId<MoodEffectPrototype>> MoodEffects { get; private set; } = new();

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        var prototype = IoCManager.Resolve<IPrototypeManager>();

        foreach (var moodProto in MoodEffects)
            if (prototype.TryIndex(moodProto, out var moodlet))
                entityManager.EventBus.RaiseLocalEvent(uid, new MoodEffectEvent(moodlet.ID));
    }
}

/// Add or remove Factions from a player upon spawning in.
[UsedImplicitly]
public sealed partial class TraitModifyFactions : TraitFunction
{
    /// <summary>
    ///     The list of all Factions that this trait removes.
    /// </summary>
    /// <remarks>
    ///     I can't actually Validate these because the proto lives in Shared.
    /// </remarks>
    [DataField, AlwaysPushInheritance]
    public List<string> RemoveFactions { get; private set; } = new();

    /// <summary>
    ///     The list of all Factions that this trait adds.
    /// </summary>
    /// <remarks>
    ///     I can't actually Validate these because the proto lives in Shared.
    /// </remarks>
    [DataField, AlwaysPushInheritance]
    public List<string> AddFactions { get; private set; } = new();

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        var factionSystem = entityManager.System<NpcFactionSystem>();

        foreach (var faction in RemoveFactions)
            factionSystem.RemoveFaction(uid, faction);

        foreach (var faction in AddFactions)
            factionSystem.AddFaction(uid, faction);
    }
}

/// Only use this if you know what you're doing and there is no reasonable alternative. This function directly writes to any arbitrary component.
[UsedImplicitly]
public sealed partial class TraitVVEdit : TraitFunction
{
    [DataField, AlwaysPushInheritance]
    public Dictionary<string, string> Changes { get; private set; } = new();

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        var viewVariablesManager = IoCManager.Resolve<IViewVariablesManager>();

        foreach (var (path, value) in Changes)
        {
            var idPath = path.Replace("$ID", uid.ToString());

            viewVariablesManager.WritePath(idPath, value);
        }
    }
}

/// Only use this if you know what you're doing and there is no reasonable alternative. This function directly writes to any arbitrary component, relative to the current value. Only works for floats.
[UsedImplicitly]
public sealed partial class TraitVVModify : TraitFunction
{
    [DataField, AlwaysPushInheritance]
    public Dictionary<string, float> Changes { get; private set; } = new();

    [DataField, AlwaysPushInheritance]
    public bool Multiply { get; private set; } = false; // Should the value be multiplied compared to the current one? If not, add/subtract instead.

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        var viewVariablesManager = IoCManager.Resolve<IViewVariablesManager>();

        foreach (var (path, value) in Changes)
        {
            var idPath = path.Replace("$ID", uid.ToString());

            if (!float.TryParse(viewVariablesManager.ReadPathSerialized(idPath), out var currentValue))
                continue;

            float newValue = currentValue + value;

            if (Multiply)
                newValue = currentValue * value;

            viewVariablesManager.WritePath(idPath, newValue.ToString());
        }
    }
}

/// Used for writing to an entity's ExtendDescriptionComponent. If one is not present, it will be added!
/// Use this to create traits that add special descriptions for when a character is shift-click examined.
[UsedImplicitly]
public sealed partial class TraitPushDescription : TraitFunction
{
    [DataField, AlwaysPushInheritance]
    public List<DescriptionExtension> DescriptionExtensions { get; private set; } = new();

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        entityManager.EnsureComponent<ExtendDescriptionComponent>(uid, out var descComp);
        foreach (var descExtension in DescriptionExtensions)
            descComp.DescriptionList.Add(descExtension);
    }
}

[UsedImplicitly]
public sealed partial class TraitAddArmor : TraitFunction
{
    /// <summary>
    ///     The list of prototype ID's of DamageModifierSets to be added to the enumerable damage modifiers of an entity.
    /// </summary>
    /// <remarks>
    ///     Dear Maintainer, I'm well aware that validating protoIds is a thing. Unfortunately, this is for a legacy system that doesn't have validated prototypes.
    ///     And refactoring the entire DamageableSystem is way the hell outside of the scope of the PR adding this function.
    ///     {FaridaIsCute.png} - Solidus
    /// </remarks>
    [DataField, AlwaysPushInheritance]
    public List<string> DamageModifierSets { get; private set; } = new();

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        entityManager.EnsureComponent<DamageableComponent>(uid, out var damageableComponent);
        foreach (var modifierSet in DamageModifierSets)
            damageableComponent.DamageModifierSets.Add(modifierSet);

        // These functions live in the Server, but these components are Shared, so we gotta dirty so that prediction will work.
        entityManager.Dirty(uid, damageableComponent);
    }
}

[UsedImplicitly]
public sealed partial class TraitRemoveArmor : TraitFunction
{
    [DataField, AlwaysPushInheritance]
    public List<string> DamageModifierSets { get; private set; } = new();

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        if (!entityManager.TryGetComponent<DamageableComponent>(uid, out var damageableComponent))
            return;

        foreach (var modifierSet in DamageModifierSets)
            damageableComponent.DamageModifierSets.Remove(modifierSet);

        // These functions live in the Server, but these components are Shared, so we gotta dirty so that prediction will work.
        entityManager.Dirty(uid, damageableComponent);
    }
}

[UsedImplicitly]
public sealed partial class TraitAddSolutionContainer : TraitFunction
{
    [DataField, AlwaysPushInheritance]
    public Dictionary<string, SolutionComponent> Solutions { get; private set; } = new();

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        var solutionContainer = entityManager.System<SharedSolutionContainerSystem>();

        foreach (var (containerKey, solution) in Solutions)
        {
            var hasSolution = solutionContainer.EnsureSolution(uid, containerKey, out Solution? newSolution);

            if (!hasSolution)
                return;

            newSolution!.AddSolution(solution.Solution, null);
        }
        // No need to dirty here since EnsureSolution and AddSolution already handle that.
    }
}

[UsedImplicitly]
public sealed partial class TraitModifyMobThresholds : TraitFunction
{
    [DataField, AlwaysPushInheritance]
    public int CritThresholdModifier;

    [DataField, AlwaysPushInheritance]
    public int SoftCritThresholdModifier;

    [DataField, AlwaysPushInheritance]
    public int DeadThresholdModifier;

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        if (!entityManager.TryGetComponent<MobThresholdsComponent>(uid, out var threshold))
            return;

        var thresholdSystem = entityManager.System<MobThresholdSystem>();
        if (CritThresholdModifier != 0)
        {
            var critThreshold = thresholdSystem.GetThresholdForState(uid, MobState.Critical, threshold);
            if (critThreshold != 0)
                thresholdSystem.SetMobStateThreshold(uid, critThreshold + CritThresholdModifier, MobState.Critical);
        }

        if (SoftCritThresholdModifier != 0)
        {
            var softCritThreshold = thresholdSystem.GetThresholdForState(uid, MobState.SoftCritical, threshold);
            if (softCritThreshold != 0)
                thresholdSystem.SetMobStateThreshold(uid, softCritThreshold + SoftCritThresholdModifier, MobState.SoftCritical);
        }

        if (DeadThresholdModifier != 0)
        {
            var deadThreshold = thresholdSystem.GetThresholdForState(uid, MobState.Dead, threshold);
            if (deadThreshold != 0)
                thresholdSystem.SetMobStateThreshold(uid, deadThreshold + DeadThresholdModifier, MobState.Dead);
        }
        // We don't need to Dirty here, since SetMobStateThreshold already has a Dirty(uid, comp) in it.
    }
}

[UsedImplicitly]
public sealed partial class TraitModifyMobState : TraitFunction
{
    // Three-State Booleans my beloved.
    // :faridabirb.png:

    [DataField, AlwaysPushInheritance]
    public bool? AllowMovementWhileCrit;

    [DataField, AlwaysPushInheritance]
    public bool? AllowMovementWhileSoftCrit;

    [DataField, AlwaysPushInheritance]
    public bool? AllowMovementWhileDead;

    [DataField, AlwaysPushInheritance]
    public bool? AllowTalkingWhileCrit;

    [DataField, AlwaysPushInheritance]
    public bool? AllowTalkingWhileSoftCrit;

    [DataField, AlwaysPushInheritance]
    public bool? AllowTalkingWhileDead;

    [DataField, AlwaysPushInheritance]
    public bool? DownWhenCrit;

    [DataField, AlwaysPushInheritance]
    public bool? DownWhenSoftCrit;

    [DataField, AlwaysPushInheritance]
    public bool? DownWhenDead;

    [DataField, AlwaysPushInheritance]
    public bool? AllowHandInteractWhileCrit;

    [DataField, AlwaysPushInheritance]
    public bool? AllowHandInteractWhileSoftCrit;

    [DataField, AlwaysPushInheritance]
    public bool? AllowHandInteractWhileDead;

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        if (!entityManager.TryGetComponent<MobStateComponent>(uid, out var mobStateComponent))
            return;

        if (AllowMovementWhileCrit is not null)
            mobStateComponent.AllowMovementWhileCrit = AllowMovementWhileCrit.Value;

        if (AllowMovementWhileSoftCrit is not null)
            mobStateComponent.AllowMovementWhileSoftCrit = AllowMovementWhileSoftCrit.Value;

        if (AllowMovementWhileDead is not null)
            mobStateComponent.AllowMovementWhileDead = AllowMovementWhileDead.Value;

        if (AllowTalkingWhileCrit is not null)
            mobStateComponent.AllowTalkingWhileCrit = AllowTalkingWhileCrit.Value;

        if (AllowTalkingWhileSoftCrit is not null)
            mobStateComponent.AllowTalkingWhileSoftCrit = AllowTalkingWhileSoftCrit.Value;

        if (AllowTalkingWhileDead is not null)
            mobStateComponent.AllowTalkingWhileDead = AllowTalkingWhileDead.Value;

        if (DownWhenCrit is not null)
            mobStateComponent.DownWhenCrit = DownWhenCrit.Value;

        if (DownWhenSoftCrit is not null)
            mobStateComponent.DownWhenSoftCrit = DownWhenSoftCrit.Value;

        if (DownWhenDead is not null)
            mobStateComponent.DownWhenDead = DownWhenDead.Value;

        if (AllowHandInteractWhileCrit is not null)
            mobStateComponent.AllowHandInteractWhileCrit = AllowHandInteractWhileCrit.Value;

        if (AllowHandInteractWhileSoftCrit is not null)
            mobStateComponent.AllowHandInteractWhileSoftCrit = AllowHandInteractWhileSoftCrit.Value;

        if (AllowHandInteractWhileDead is not null)
            mobStateComponent.AllowHandInteractWhileDead = AllowHandInteractWhileDead.Value;

        // These functions live in the Server, but these components are Shared, so we gotta dirty so that prediction will work.
        entityManager.Dirty(uid, mobStateComponent);
    }
}

[UsedImplicitly]
public sealed partial class TraitModifyStamina : TraitFunction
{
    [DataField, AlwaysPushInheritance]
    public float StaminaModifier;

    [DataField, AlwaysPushInheritance]
    public float DecayModifier;

    [DataField, AlwaysPushInheritance]
    public float CooldownModifier;

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        if (!entityManager.TryGetComponent<StaminaComponent>(uid, out var staminaComponent))
            return;

        staminaComponent.CritThreshold += StaminaModifier;
        staminaComponent.Decay += DecayModifier;
        staminaComponent.Cooldown += CooldownModifier;

        // No need to dirty here since this component is Auto-Networked.
    }
}

[UsedImplicitly]
public sealed partial class TraitModifyDensity : TraitFunction
{
    [DataField, AlwaysPushInheritance]
    public float DensityModifier;

    [DataField, AlwaysPushInheritance]
    public bool Multiply = false;

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        var physicsSystem = entityManager.System<SharedPhysicsSystem>();
        if (!entityManager.TryGetComponent<FixturesComponent>(uid, out var fixturesComponent)
            || fixturesComponent.Fixtures.Count is 0)
            return;

        var fixture = fixturesComponent.Fixtures.First();
        var newDensity = Multiply ? fixture.Value.Density * DensityModifier : fixture.Value.Density + DensityModifier;
        physicsSystem.SetDensity(uid, fixture.Key, fixture.Value, newDensity);
        // SetDensity handles the Dirty.
    }
}

/// <summary>
///     Used for traits that modify SlowOnDamageComponent.
/// </summary>
[UsedImplicitly]
public sealed partial class TraitModifySlowOnDamage : TraitFunction
{
    // <summary>
    //     A flat modifier to add to all damage threshold keys.
    // </summary>
    [DataField, AlwaysPushInheritance]
    public float DamageThresholdsModifier;

    // <summary>
    //     A multiplier applied to all speed modifier values.
    //     The higher the multiplier, the stronger the slowdown.
    // </summary>
    [DataField, AlwaysPushInheritance]
    public float SpeedModifierMultiplier = 1f;

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        if (!entityManager.TryGetComponent<SlowOnDamageComponent>(uid, out var slowOnDamage))
            return;

        var newSpeedModifierThresholds = new Dictionary<FixedPoint2, float>();

        foreach (var (damageThreshold, speedModifier) in slowOnDamage.SpeedModifierThresholds)
            newSpeedModifierThresholds[damageThreshold + DamageThresholdsModifier] = 1 - (1 - speedModifier) * SpeedModifierMultiplier;

        slowOnDamage.SpeedModifierThresholds = newSpeedModifierThresholds;

        // These functions live in the Server, but these components are Shared, so we gotta dirty so that prediction will work.
        entityManager.Dirty(uid, slowOnDamage);
    }
}

/// <summary>
///     Used for traits that modify unarmed damage on MeleeWeaponComponent.
/// </summary>
[UsedImplicitly]
public sealed partial class TraitModifyUnarmed : TraitFunction
{
    // <summary>
    //     The sound played on hitting targets.
    // </summary>
    [DataField, AlwaysPushInheritance]
    public SoundSpecifier? SoundHit;

    // <summary>
    //     The animation to play on hit, for both light and power attacks.
    // </summary>
    [DataField, AlwaysPushInheritance]
    public EntProtoId? Animation;

    // <summary>
    //     Whether to set the power attack animation to be the same as the light attack.
    // </summary>
    [DataField, AlwaysPushInheritance]
    public bool HeavyAnimationFromLight = true;

    // <summary>
    //     The damage values of unarmed damage.
    // </summary>
    [DataField, AlwaysPushInheritance]
    public DamageSpecifier? Damage;

    // <summary>
    //     Additional damage added to the existing damage.
    // </summary>
    [DataField, AlwaysPushInheritance]
    public DamageSpecifier? FlatDamageIncrease;

    /// <summary>
    ///   Turns the left click into a power attack when the light attack misses.
    /// </summary>
    [DataField]
    public bool? HeavyOnLightMiss;

    // <summary>
    //     What to multiply the melee weapon range by.
    // </summary>
    [DataField, AlwaysPushInheritance]
    public float? RangeModifier;

    // <summary>
    //     What to multiply the attack rate by.
    // </summary>
    [DataField, AlwaysPushInheritance]
    public float? AttackRateModifier;

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        if (!entityManager.TryGetComponent<MeleeWeaponComponent>(uid, out var melee))
            return;

        if (SoundHit != null)
            melee.SoundHit = SoundHit;

        if (Animation != null)
            melee.Animation = Animation.Value;

        if (HeavyAnimationFromLight)
            melee.WideAnimation = melee.Animation;

        if (Damage != null)
            melee.Damage = Damage;

        if (FlatDamageIncrease != null)
            melee.Damage += FlatDamageIncrease;

        if (HeavyOnLightMiss != null)
            melee.HeavyOnLightMiss = HeavyOnLightMiss.Value;

        if (RangeModifier != null)
            melee.Range *= RangeModifier.Value;

        if (AttackRateModifier != null)
            melee.AttackRate *= AttackRateModifier.Value;

        entityManager.Dirty(uid, melee);
    }
}


// <summary>
// Adds a Tag to something
// </summary>
[UsedImplicitly]
public sealed partial class TraitAddTag : TraitFunction
{
    [DataField, AlwaysPushInheritance]
    public List<ProtoId<TagPrototype>> Tags { get; private set; } = new();

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        var tagSystem = entityManager.System<TagSystem>();
        tagSystem.AddTags(uid, Tags);
    }
}

// <summary>
//      Replaces a body part with a cybernetic. This is only for limbs such as arms and legs, don't use this for organs(old or new).
// </summary>
[UsedImplicitly]
public sealed partial class TraitCyberneticLimbReplacement : TraitFunction
{
    [DataField, AlwaysPushInheritance]
    public BodyPartType RemoveBodyPart { get; private set; } = BodyPartType.Arm;

    [DataField, AlwaysPushInheritance]
    public BodyPartSymmetry PartSymmetry { get; private set; } = BodyPartSymmetry.Left;

    [DataField, AlwaysPushInheritance]
    public EntProtoId? ProtoId { get; private set; }

    [DataField, AlwaysPushInheritance]
    public string SlotId { get; private set; } = "right arm";

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        var bodySystem = entityManager.System<BodySystem>();
        var transformSystem = entityManager.System<SharedTransformSystem>();

        if (!entityManager.TryGetComponent(uid, out BodyComponent? body)
            || !entityManager.TryGetComponent(uid, out TransformComponent? xform)
            || ProtoId is null)
            return;

        var root = bodySystem.GetRootPartOrNull(uid, body);
        if (root is null)
            return;

        var parts = bodySystem.GetBodyChildrenOfType(uid, RemoveBodyPart, body);
        foreach (var part in parts)
        {
            var partComp = part.Component;
            if (partComp.Symmetry != PartSymmetry)
                continue;

            foreach (var child in bodySystem.GetBodyPartChildren(part.Id, part.Component))
                entityManager.QueueDeleteEntity(child.Id);

            transformSystem.AttachToGridOrMap(part.Id);
            entityManager.QueueDeleteEntity(part.Id);

            var newLimb = entityManager.SpawnAtPosition(ProtoId, xform.Coordinates);
            if (entityManager.TryGetComponent(newLimb, out BodyPartComponent? limbComp))
                bodySystem.AttachPart(root.Value.Entity, SlotId, newLimb, root.Value.BodyPart, limbComp);
        }
    }
}
