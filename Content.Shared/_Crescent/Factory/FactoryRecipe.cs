using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared.Factory.Components
{
    [Prototype("factoryRecipe"), Serializable, NetSerializable]
    public sealed partial class FactoryRecipe : IPrototype
    {
        [IdDataField]
        public string ID { get; private set; } = default!;

        [DataField("name")]
        public string name = "";

        [DataField("inputs")]
        public Dictionary<string, int> Inputs = new();

        [DataField("outputs")]
        public Dictionary<string, int> Outputs = new();
    }
}
