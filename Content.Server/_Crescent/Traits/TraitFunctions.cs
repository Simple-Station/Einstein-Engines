using System.Linq;
using Newtonsoft.Json;
using JetBrains.Annotations;
using Content.Shared.Traits.Assorted.Components;
using Robust.Shared.Serialization.Manager;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Traits;
using Robust.Shared.Prototypes;
using Content.Shared.Body.Part;
using Content.Server.Body.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Body.Organ;

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

/// <summary>
///      Replaces an organ with a cybernetic. This is only for organs, don't use this for limbs(old or new).
/// </summary>
[UsedImplicitly]
public sealed partial class TraitCyberneticOrganReplacement : TraitFunction
{
    [DataField, AlwaysPushInheritance]
    public BodyPartType BodyPart { get; private set; } = BodyPartType.Torso;

    [DataField, AlwaysPushInheritance]
    public BodyPartSymmetry PartSymmetry { get; private set; } = BodyPartSymmetry.None;

    [DataField, AlwaysPushInheritance]
    public EntProtoId? ProtoId { get; private set; }

    [DataField, AlwaysPushInheritance]
    public string SlotId { get; private set; } = "heart";

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

        var newOrgan = entityManager.SpawnAtPosition(ProtoId, xform.Coordinates);
        if (!entityManager.TryGetComponent(newOrgan, out OrganComponent? newOrganComp))
            return;

        var part = bodySystem.GetBodyChildrenOfType(uid, BodyPart, body, PartSymmetry);

        var organs = bodySystem.GetPartOrgans(part.FirstOrDefault().Id, part.FirstOrDefault().Component);
        foreach (var organ in organs)
        {
            if (organ.Component.SlotId == newOrganComp.SlotId)
            {
                transformSystem.AttachToGridOrMap(organ.Id);
                entityManager.QueueDeleteEntity(organ.Id);
                break;
            }
        }
        bodySystem.InsertOrgan(part.FirstOrDefault().Id, newOrgan, slotId: SlotId, part.FirstOrDefault().Component, newOrganComp);
    }
}
