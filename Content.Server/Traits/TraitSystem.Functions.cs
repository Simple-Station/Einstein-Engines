using Content.Shared.Traits;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Content.Shared.Implants;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;
using Content.Shared.Actions;

namespace Content.Server.Traits;

/// <summary>
///     Used for traits that add a Component upon spawning in, overwriting the pre-existing component if it already exists.
/// </summary>
[UsedImplicitly]
public sealed partial class TraitReplaceComponent : TraitFunction
{
    [AlwaysPushInheritance]
    public ComponentRegistry Components { get; private set; } = new();

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        foreach (var (name, data) in Components)
        {
            var component = (Component) factory.GetComponent(name);
            component.Owner = uid;

            var temp = (object) component;
            serializationManager.CopyTo(data.Component, ref temp);
            entityManager.RemoveComponent(uid, temp!.GetType());
            entityManager.AddComponent(uid, (Component) temp);
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
    [AlwaysPushInheritance]
    public ComponentRegistry Components { get; private set; } = new();

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        foreach (var (name, _) in Components)
        {
            var component = (Component) factory.GetComponent(name);
            component.Owner = uid;

            entityManager.AddComponent(uid, component);
        }
    }
}

/// <summary>
///     Used for traits that remove a component upon a player spawning in.
/// </summary>
[UsedImplicitly]
public sealed partial class TraitRemoveComponent : TraitFunction
{
    [AlwaysPushInheritance]
    public ComponentRegistry Components { get; private set; } = new();

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        foreach (var (name, _) in Components)
            entityManager.RemoveComponent(uid, (Component) factory.GetComponent(name));
    }
}

/// <summary>
///     Used for traits that remove a component upon a player spawning in.
/// </summary>
[UsedImplicitly]
public sealed partial class TraitAddActions : TraitFunction
{
    [AlwaysPushInheritance]
    public List<EntProtoId>? Actions { get; private set; } = default!;

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        if (Actions is null)
            return;

        var actionSystem = entityManager.System<SharedActionsSystem>();

        foreach (var id in Actions)
        {
            EntityUid? actionId = null;
            if (actionSystem.AddAction(uid, ref actionId, id))
                actionSystem.StartUseDelay(actionId);
        }
    }
}

/// <summary>
///     Used for traits that add an Implant upon spawning in.
/// </summary>
[UsedImplicitly]
public sealed partial class TraitAddImplant : TraitFunction
{
    [DataField(customTypeSerializer: typeof(PrototypeIdHashSetSerializer<EntityPrototype>))]
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
