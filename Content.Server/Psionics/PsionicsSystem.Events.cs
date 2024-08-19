
namespace Content.Server.Psionics
{
    /// <summary>
    ///     Raised on an entity about to roll for a Psionic Power, after their baseline chances of success are calculated.
    /// </summary>
    [ByRefEvent]
    public struct OnRollPsionicsEvent
    {
        public readonly EntityUid Roller;
        public float BaselineChance;
        public OnRollPsionicsEvent(EntityUid roller, float baselineChance)
        {
            Roller = roller;
            BaselineChance = baselineChance;
        }
    }
}

