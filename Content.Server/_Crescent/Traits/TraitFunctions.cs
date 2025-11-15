using System.Collections.Generic;
using System.Linq;
using Robust.Shared.GameObjects;
using Newtonsoft.Json;
using Robust.Shared.Serialization.Manager.Attributes;
using JetBrains.Annotations;
using Content.Shared.Traits.Assorted.Components;
using Robust.Shared.Serialization.Manager;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Traits;

namespace Content.Shared.Traits
{
    [UsedImplicitly]
    public sealed partial class TraitPopDescription : TraitFunction
    {
        [DataField, AlwaysPushInheritance]
        public List<DescriptionExtension> DescriptionExtensions { get; private set; } = new();

        public override void OnPlayerSpawn(EntityUid uid,
            IComponentFactory factory,
            IEntityManager entityManager,
            ISerializationManager serializationManager)
        {
            if (!entityManager.TryGetComponent<ExtendDescriptionComponent>(uid, out var descComp))
                return;

            foreach (var descExtension in DescriptionExtensions)
            {
                var toRemove = descComp.DescriptionList.FirstOrDefault(ext => JsonConvert.SerializeObject(descExtension) == JsonConvert.SerializeObject(ext)); // the worst hack I have ever written but I have to do this
                if (toRemove != null)
                    descComp.DescriptionList.Remove(toRemove);
            }
        }
    }
}

/// <summary>
///     Used for cybernetics that add a slot upon spawning in.
///     If there's a slot with the ID you're trying to add, it does nothing to that slot.
/// </summary>
[UsedImplicitly]
public sealed partial class TraitAddSlot : TraitFunction
{
    [DataField, AlwaysPushInheritance]
    public Dictionary<string, ItemSlot> Slots { get; private set; } = new();

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        var slotSystem = entityManager.System<ItemSlotsSystem>();

        foreach (var (slotId, slot) in Slots)
        {
            if (slotSystem.TryGetSlot(uid, slotId, out _))
                continue;

            slotSystem.AddItemSlot(uid, slotId, slot);
        }
    }
}

/// Inverse of TraitAddSlot. Need I say more?
[UsedImplicitly]
public sealed partial class TraitRemoveSlot : TraitFunction
{
    [DataField, AlwaysPushInheritance]
    public List<string> Slots { get; private set; } = new();

    public override void OnPlayerSpawn(EntityUid uid,
        IComponentFactory factory,
        IEntityManager entityManager,
        ISerializationManager serializationManager)
    {
        var slotSystem = entityManager.System<ItemSlotsSystem>();

        foreach (var slotId in Slots)
        {
            if (!slotSystem.TryGetSlot(uid, slotId, out var itemSlot))
                continue;

            slotSystem.RemoveItemSlot(uid, itemSlot);
        }
    }
}
