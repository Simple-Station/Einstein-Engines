using Robust.Shared.Serialization;
using Robust.Shared.Prototypes;
using Content.Shared.Materials;

namespace Content.Shared._NF.Market;

public abstract class SharedMarketSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    private ISawmill _log = default!;

    public int MaterialVolumeToAmount(MaterialPrototype material, int volume)
    {
        if (!_prototypeManager.TryIndex(material.StackEntity, out var entity))
        {
            _log.Error("Failed to index stack entity " + material.StackEntity + ". Check material prototype " + material.ID);
            return 0;
        }

        if (!entity.TryGetComponent<PhysicalCompositionComponent>(out var physComp) || !physComp.MaterialComposition.ContainsKey(material.ID))
        {
            _log.Warning("Despite being a representative entity for a material, " + entity.ID + " does not have a physical composition containing that material.");
            return volume;
        }

        var volumePerAmount = physComp.MaterialComposition[material.ID];

        return (int) Math.Floor((float) volume / (float) volumePerAmount);
    }

    public int StandardMaterialAmount(MaterialPrototype material)
    {
        if (!_prototypeManager.TryIndex(material.StackEntity, out var stackProto))
        {
            _log.Error("Invalid stack prototype for " + material.ID);
            return 0;
        }

        if (!stackProto.TryGetComponent<PhysicalCompositionComponent>(out var physComp) || !physComp.MaterialComposition.ContainsKey(material.ID))
        {
            _log.Error("Stack entity for material " + material.ID + "doesn't contain the materials it should");
            return 0;
        }

        return physComp.MaterialComposition[material.ID];
    }
}

[NetSerializable, Serializable]
public enum MarketConsoleUiKey : byte
{
    Default
}
