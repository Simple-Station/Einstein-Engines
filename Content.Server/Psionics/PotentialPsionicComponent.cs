namespace Content.Server.Psionics
{
    [RegisterComponent]
    public sealed partial class PotentialPsionicComponent : Component
    {
        /// <summary>
        /// The base chance of an entity rolling psychic powers, which is increased by other modifiers such as glimmer.
        /// </summary>
        /// <remarks>
        /// I have increased this to 10% up from its original value of 2%, because I estimate that most people won't take the Latent Psychic trait
        /// Simply because they might not even know it exists
        /// </remarks>
        [DataField("chance")]
        public float Chance = 0.10f;

        /// <summary>
        /// YORO (you only reroll once)
        /// </summary>
        public bool Rerolled = false;
    }
}
