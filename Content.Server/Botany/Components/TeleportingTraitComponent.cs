namespace Content.Server.Botany
{
    [RegisterComponent]

    public sealed partial class TeleportingTraitComponent : Component
    {
        /// <summary>
        ///     Teleportation radius of produce.
        /// </summary>
        [DataField]
        public float ProduceTeleportRadius;

        /// <summary>
        ///     How much to divide the potency.
        /// </summary>
        [DataField]
        public float PotencyDivide = 10f;

        /// <summary>
        ///     Potency of fruit.
        /// </summary>
        [DataField]
        public float Potency;

        /// <summary>
        ///     Chance of deletion.
        /// </summary>
        [DataField]
        public float DeletionChance = .5f;
    }
}
