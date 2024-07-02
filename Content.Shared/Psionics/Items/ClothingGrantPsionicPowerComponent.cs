namespace Content.Shared.Psionics.Abilities
{
    [RegisterComponent]
    public sealed partial class ClothingGrantPsionicPowerComponent : Component
    {
        [DataField("power", required: true)]
        public string Power = "";
        public bool IsActive = false;
    }
}
