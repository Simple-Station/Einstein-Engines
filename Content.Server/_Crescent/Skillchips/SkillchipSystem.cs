using Content.Shared.Containers.ItemSlots;
using Robust.Shared.Containers;
using Robust.Shared.Serialization.Manager;

namespace Content.Server._Crescent.Skillchips;

public sealed partial class SkillchipSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly ISerializationManager _serializationManager = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;

    private ISawmill _logger = Logger.GetSawmill("skillchip");
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SkillchipImplantHolderComponent, ItemSlotInsertEvent>(OnSkillchipSlotInsertion);
        SubscribeLocalEvent<SkillchipImplantHolderComponent, ItemSlotEjectEvent>(OnSkillchipSlotEjection);
    }

    private void OnSkillchipSlotInsertion(Entity<SkillchipImplantHolderComponent> skillchipUser, ref ItemSlotInsertEvent args)
    {
        var skillchipHolder = args.Item;

        if (!TryComp<SkillchipHolderComponent>(skillchipHolder, out var holderComp))
            return;

        // evil loop
        for (int i = 1; i <= holderComp.SkillchipSlotAmount; i++)
        {
            var itemSlotName = $"{holderComp.SkillchipSlotPrefix}{i}";

            if (!_itemSlots.TryGetSlot(skillchipHolder, itemSlotName, out var itemSlot))
                continue;

            if (!TryComp<SkillchipComponent>(itemSlot.Item, out var comp))
                continue;

            AddFunctions(skillchipUser.Owner, comp);

            // holy shitcode
        }

    }

    private void OnSkillchipSlotEjection(Entity<SkillchipImplantHolderComponent> skillchipUser, ref ItemSlotEjectEvent args)
    {
        var skillchipHolder = args.Item;

        if (!TryComp<SkillchipHolderComponent>(skillchipHolder, out var holderComp))
            return;

        // evil loop
        for (int i = 1; i <= holderComp.SkillchipSlotAmount; i++)
        {
            var itemSlotName = $"{holderComp.SkillchipSlotPrefix}{i}";

            if (!_itemSlots.TryGetSlot(skillchipHolder, itemSlotName, out var itemSlot))
                continue;

            if (!TryComp<SkillchipComponent>(itemSlot.Item, out var comp))
                continue;

            RemoveFunctions(skillchipUser.Owner, comp);

            // holy shitcode part 2: copy-paste edition!
        }

    }

    private void AddFunctions(EntityUid playerUid, SkillchipComponent skillchip)
    {
        foreach (var function in skillchip.OnImplantFunctions)
            function.OnPlayerSpawn(playerUid, _componentFactory, EntityManager, _serializationManager);
    }
    private void RemoveFunctions(EntityUid playerUid, SkillchipComponent skillchip)
    {
        foreach (var function in skillchip.OnRemoveFunctions)
            function.OnPlayerSpawn(playerUid, _componentFactory, EntityManager, _serializationManager);
    }
}
