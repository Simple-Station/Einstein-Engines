using Content.Shared.CCVar;

namespace Content.Shared.Contests;
public sealed partial class ContestsSystem
{
    /// <summary>
    ///     Clamp a contest to a Range of [Epsilon, 32bit integer limit]. This exists to make sure contests are always "Safe" to divide by.
    /// </summary>
    private float ContestClamp(float input)
    {
        return Math.Clamp(input, float.Epsilon, float.MaxValue);
    }

    /// <summary>
    ///     Shorthand for checking if clamp overrides are allowed, and the bypass is used by a contest.
    /// </summary>
    private bool ContestClampOverride(bool bypassClamp)
    {
        return _cfg.GetCVar(CCVars.AllowClampOverride) && bypassClamp;
    }
}
