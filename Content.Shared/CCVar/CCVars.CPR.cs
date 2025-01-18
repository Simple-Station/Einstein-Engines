using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /*
        * CPR System
        */
    /// <summary>
    ///     Controls whether the entire CPR system runs. When false, nobody can perform CPR. You should probably remove the trait too
    ///     if you are wishing to permanently disable the system on your server.
    /// </summary>
    public static readonly CVarDef<bool> EnableCPR =
        CVarDef.Create("cpr.enable", true, CVar.REPLICATED | CVar.SERVER);

    /// <summary>
    ///     Toggles whether or not CPR reduces rot timers(As an abstraction of delaying brain death, the IRL actual purpose of CPR)
    /// </summary>
    public static readonly CVarDef<bool> CPRReducesRot =
        CVarDef.Create("cpr.reduces_rot", true, CVar.REPLICATED | CVar.SERVER);

    /// <summary>
    ///     Toggles whether or not CPR heals airloss, included for completeness sake. I'm not going to stop you if your intention is to make CPR do nothing.
    ///     I guess it might be funny to troll your players with? I won't judge.
    /// </summary>
    public static readonly CVarDef<bool> CPRHealsAirloss =
        CVarDef.Create("cpr.heals_airloss", true, CVar.REPLICATED | CVar.SERVER);

    /// <summary>
    ///     The chance for a patient to be resuscitated when CPR is successfully performed.
    ///     Setting this above 0 isn't very realistic, but people who see CPR in movies and TV will expect CPR to work this way.
    /// </summary>
    public static readonly CVarDef<float> CPRResuscitationChance =
        CVarDef.Create("cpr.resuscitation_chance", 0.05f, CVar.REPLICATED | CVar.SERVER);

    /// <summary>
    ///     By default, CPR reduces rot timers by an amount of seconds equal to the time spent performing CPR. This is an optional multiplier that can increase or decrease the amount
    ///     of rot reduction. Set it to 2 for if you want 3 seconds of CPR to reduce 6 seconds of rot.
    /// </summary>
    /// <remarks>
    ///     If you're wondering why there isn't a CVar for setting the duration of the doafter, that's because it's not actually possible to have a timespan in cvar form
    ///     Curiously, it's also not possible for **shared** systems to set variable timespans. Which is where this system lives.
    /// </remarks>
    public static readonly CVarDef<float> CPRRotReductionMultiplier =
        CVarDef.Create("cpr.rot_reduction_multiplier", 1f, CVar.REPLICATED | CVar.SERVER);

    /// <summary>
    ///     By default, CPR heals airloss by 1 point for every second spent performing CPR. Just like above, this directly multiplies the healing amount.
    ///     Set it to 2 to get 6 points of airloss healing for every 3 seconds of CPR.
    /// </summary>
    public static readonly CVarDef<float> CPRAirlossReductionMultiplier =
        CVarDef.Create("cpr.airloss_reduction_multiplier", 1f, CVar.REPLICATED | CVar.SERVER);
}
