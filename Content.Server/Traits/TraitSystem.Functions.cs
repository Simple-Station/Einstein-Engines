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
using Content.Server.NPC.Systems;

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

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        var prototype = IoCManager.Resolve<IPrototypeManager>();
        var psionic = entityManager.System<PsionicAbilitiesSystem>();

        foreach (var powerProto in PsionicPowers)
            if (prototype.TryIndex(powerProto, out var psionicPower))
                psionic.InitializePsionicPower(uid, psionicPower, false);
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

/// Only use this if you know what you're doing. This function directly writes to any arbitrary component.
[UsedImplicitly]
public sealed partial class TraitVVEdit : TraitFunction
{
    [DataField, AlwaysPushInheritance]
    public Dictionary<string, string> VVEdit { get; private set; } = new();

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        var vvm = IoCManager.Resolve<IViewVariablesManager>();
        foreach (var (path, value) in VVEdit)
            vvm.WritePath(path, value);
    }
}
