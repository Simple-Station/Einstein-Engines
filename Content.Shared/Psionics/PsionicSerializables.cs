namespace Content.Shared.Psionics;

/// <summary>
///     This event is raised whenever a psionic entity sets their casting stats(Amplification and Dampening), allowing other systems to modify the end result
///     of casting stat math. Useful if for example you want a species to have 5% higher Amplification overall. Or a drug inhibits total Dampening, etc.
/// </summary>
/// <param name="receiver"></param>
/// <param name="amplificationChangedAmount"></param>
/// <param name="dampeningChangedAmount"></param>
[ByRefEvent]
public struct OnSetPsionicStatsEvent
{
    public float AmplificationChangedAmount;
    public float DampeningChangedAmount;

    /// <summary>
    ///     This event is raised whenever a psionic entity sets their casting stats(Amplification and Dampening), allowing other systems to modify the end result
    ///     of casting stat math. EG: The end result after tallying up
    /// </summary>
    /// <param name="receiver"></param>
    /// <param name="amplificationChangedAmount"></param>
    /// <param name="dampeningChangedAmount"></param>
    public OnSetPsionicStatsEvent(float amplificationChangedAmount, float dampeningChangedAmount)
    {
        AmplificationChangedAmount = amplificationChangedAmount;
        DampeningChangedAmount = dampeningChangedAmount;
    }
}