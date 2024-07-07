using Robust.Shared.GameObjects;
using Robust.Shared.Serialization.Manager.Attributes;

namespace Content.Shared.Key
{
    [RegisterComponent]
    public partial class KeyComponent : Component
    {

        [DataField("keyId")]
        public string KeyId = string.Empty;
    }
}
