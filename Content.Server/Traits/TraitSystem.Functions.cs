using Content.Shared.Traits;
using JetBrains.Annotations;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Content.Shared.Implants;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.Set;

namespace Content.Server.Traits;

/// <summary>
///     Used for traits that add a Component upon spawning in.
/// </summary>
[UsedImplicitly]
public sealed partial class TraitAddComponent : TraitFunction
{
    [AlwaysPushInheritance]
    public ComponentRegistry Components { get; private set; } = new();

    public override void OnPlayerSpawn(EntityUid mob)
    {
        var factory = IoCManager.Resolve<IComponentFactory>();
        var entityManager = IoCManager.Resolve<IEntityManager>();
        var serializationManager = IoCManager.Resolve<ISerializationManager>();

        foreach (var (name, data) in Components)
        {
            var component = (Component) factory.GetComponent(name);
            component.Owner = mob;

            var temp = (object) component;
            serializationManager.CopyTo(data.Component, ref temp);
            entityManager.RemoveComponent(mob, temp!.GetType());
            entityManager.AddComponent(mob, (Component) temp);
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

    public override void OnPlayerSpawn(EntityUid mob)
    {
        var entMan = IoCManager.Resolve<IEntityManager>();
        var implantSystem = entMan.System<SharedSubdermalImplantSystem>();
        implantSystem.AddImplants(mob, Implants);
    }
}
