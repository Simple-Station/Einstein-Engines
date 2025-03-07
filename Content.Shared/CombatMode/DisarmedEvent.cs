namespace Content.Shared.CombatMode
{
    public sealed class DisarmedEvent : HandledEntityEventArgs
    {
        /// <summary>
        ///     The entity being disarmed.
        /// </summary>
        public EntityUid Target { get; init; }

        /// <summary>
        ///     The entity performing the disarm.
        /// </summary>
        public EntityUid Source { get; init; }

        /// <summary>
        ///    WWDP - Probability to disarm in addition to shoving.
        /// </summary>
        public float DisarmProbability { get; init; }

        /// <summary>
        ///     Potential stamina damage if this disarm results in a shove.
        /// </summary>
        public float StaminaDamage { get; init; }
    }
}
