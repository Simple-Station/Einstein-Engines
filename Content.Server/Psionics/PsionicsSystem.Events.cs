
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

    /// <summary>
    ///     Raised on an entity when a Potentiometer has finished scanning it, but before the check for no feedback messages is made.
    ///     This allows entities to either mess with the Examiner, or modify the list.
    /// </summary>
    /// <param name="Examiner"></param>
    [ByRefEvent]
    public record struct OnPotentiometryEvent(EntityUid Examiner, List<string> Messages);
}

