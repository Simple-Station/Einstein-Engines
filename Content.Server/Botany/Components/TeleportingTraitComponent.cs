namespace Content.Server.Botany
{
    [RegisterComponent]

    public sealed partial class TeleportingTraitComponent : Component
    {
        ///<summary>
        /// Teleportation radius of produce.
        ///
        [DataField("produceTeleportRadius")]
        public float ProduceTeleportRadius;

        ///<summary>
        /// How much to divide the potency.
        ///
        [DataField("potencyDivide")]
        public float PotencyDivide = 10f;

        ///<summary>
        ///  Potency of fruit.
        ///
        [DataField("potency")]
        public float Potency;

        ///<summary>
        ///  Chance of deletion.
        ///
        [DataField("deletionChance")]
        public float DeletionChance = .5f;
    }
}
