using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.Key
{
    [RegisterComponent]
    public sealed partial class KeyRequiredComponent : Component
    {

        [DataField("requiredKeyId")]
        public string RequiredKeyId = string.Empty;
    }
}
