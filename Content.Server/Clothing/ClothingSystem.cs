using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Inventory.Events;
using Robust.Shared.Serialization.Manager;

namespace Content.Server.Clothing;

public sealed class ServerClothingSystem : ClothingSystem
{
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly ISerializationManager _serializationManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ClothingComponent, DidEquipEvent>(OnClothingEquipped);
        SubscribeLocalEvent<ClothingComponent, DidUnequipEvent>(OnClothingUnequipped);
    }

    private void OnClothingEquipped(EntityUid uid, ClothingComponent clothingComponent, DidEquipEvent args)
    {
        // Yes, this is using the trait system functions. I'm not going to write an entire third function library to do this.
        // They're generic for a reason.
        foreach (var function in clothingComponent.OnEquipFunctions)
            function.OnPlayerSpawn(uid, _componentFactory, EntityManager, _serializationManager);
    }

    private void OnClothingUnequipped(EntityUid uid, ClothingComponent clothingComponent, DidUnequipEvent args)
    {
        // Yes, this is using the trait system functions. I'm not going to write an entire third function library to do this.
        // They're generic for a reason.
        foreach (var function in clothingComponent.OnUnequipFunctions)
            function.OnPlayerSpawn(uid, _componentFactory, EntityManager, _serializationManager);
    }
}
