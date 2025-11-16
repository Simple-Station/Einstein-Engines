using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Crescent.SpaceBiomes;

[RegisterComponent, AutoGenerateComponentState]
public sealed partial class SpaceBiomeSourceComponent : Component
{
    [DataField(required: true, customTypeSerializer: typeof(PrototypeIdSerializer<SpaceBiomePrototype>)), AutoNetworkedField]
    public string Biome = "";

    /// <summary>
    /// Distance at which swap should begin
    /// Since system is updated once in several seconds it may happen significantly later, so set this to atleast 100-150m
    /// </summary>
    [DataField(required: true)]
    public int SwapDistance;

    /// <summary>
    /// If multiple biomes are overlapping, biome with the highest priority is applied
    /// </summary>
    [DataField]
    public int Priority;
}
