using Robust.Shared.GameObjects;

namespace Content.Shared.Vehicle
{
    [RegisterComponent]
    public partial class KeyComponent : Component
    {
        [DataField("keyType")]
        public string KeyType = string.Empty;

        [ViewVariables]
        public bool IsInserted = false;
    }
}
