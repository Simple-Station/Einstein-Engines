using Content.Shared.Psionics;

namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class MetapsionicPowerComponent : Component
    {
        [DataField]
        public float Range = 5f;
    }
}
