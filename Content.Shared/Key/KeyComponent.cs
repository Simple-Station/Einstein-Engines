using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.Key
{
    [RegisterComponent]
    public partial class KeyComponent : Component
    {
        public override string Name => "Key";

        [DataField("keyId")]
        public string KeyId = string.Empty;
    }
}
