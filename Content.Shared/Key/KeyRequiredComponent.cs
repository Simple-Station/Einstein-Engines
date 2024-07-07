using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.Key
{
    [RegisterComponent]
    public partial class KeyRequiredComponent : Component
    {
        public override string Name => "KeyRequired";

        [DataField("requiredKeyId")]
        public string RequiredKeyId = string.Empty;
    }
}
