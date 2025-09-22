using Robust.Shared.Containers;
using Robust.Shared.Serialization.Manager;

namespace Content.Server._Crescent.Skillchips;

public sealed partial class SkillchipSystem : EntitySystem
{
    [Dependency] private readonly IComponentFactory _componentFactory = default!;
    [Dependency] private readonly ISerializationManager _serializationManager = default!;

    private ISawmill _logger = Logger.GetSawmill("skillchip");
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SkillchipImplantHolderComponent, EntInsertedIntoContainerMessage>(OnSkillchipContainerInserted);
    }

    private void OnSkillchipContainerInserted(Entity<SkillchipImplantHolderComponent> skillchipUser, ref EntInsertedIntoContainerMessage args)
    {
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
