namespace Content.Shared.Psionics.Abilities
{
    [RegisterComponent]
    public sealed partial class PsionicInsulationComponent : Component
    {
        public bool Passthrough = false;

        public List<String> SuppressedFactions = new();

        [DataField]
        public bool MindBroken = false;
    }
}
