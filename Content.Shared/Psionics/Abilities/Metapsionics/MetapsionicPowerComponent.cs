namespace Content.Shared.Abilities.Psionics
{
    [RegisterComponent]
    public sealed partial class MetapsionicPowerComponent : Component
    {
        [DataField]
        public readonly float Range = 5f;
    }
}
