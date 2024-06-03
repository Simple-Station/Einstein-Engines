using System.Linq;
using Content.Shared.CCVar;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Content.Shared.Traits;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Customization.Systems;


[ImplicitDataDefinitionForInheritors, MeansImplicitUse]
[Serializable, NetSerializable]
public abstract partial class CharacterRequirement
{
    /// <summary>
    ///     If true valid requirements will be treated as invalid and vice versa
    /// </summary>
    [DataField]
    public bool Inverted;

    /// <summary>
    ///     Checks if this character requirement is valid for the given parameters
    /// </summary>
    /// <param name="reason">Description for the requirement, shown when not null</param>
    public abstract bool IsValid(
        IPrototype prototype,
        JobPrototype job,
        HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        IEntityManager entityManager,
        IPrototypeManager prototypeManager,
        IConfigurationManager configManager,
        out FormattedMessage? reason
    );
}


#region HumanoidCharacterProfile

/// <summary>
///     Requires the profile to be within an age range
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterAgeRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public int Min;

    [DataField(required: true)]
    public int Max;

    public override bool IsValid(IPrototype prototype, JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason)
    {
        reason = FormattedMessage.FromMarkup(Loc.GetString("character-age-requirement",
            ("min", Min), ("max", Max)));
        return profile.Age >= Min && profile.Age <= Max;
    }
}

/// <summary>
///   Requires the profile to use either a Backpack, Satchel, or Duffelbag
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterBackpackTypeRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public BackpackPreference Preference;

    public override bool IsValid(IPrototype prototype, JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason)
    {
        reason = FormattedMessage.FromMarkup(Loc.GetString("character-backpack-type-requirement",
            ("type", Loc.GetString($"humanoid-profile-editor-preference-{Preference.ToString().ToLower()}"))));
        return profile.Backpack == Preference;
    }
}

/// <summary>
///     Requires the profile to use either Jumpsuits or Jumpskirts
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterClothingPreferenceRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public ClothingPreference Preference;

    public override bool IsValid(IPrototype prototype, JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason)
    {
        reason = FormattedMessage.FromMarkup(Loc.GetString("character-clothing-preference-requirement",
            ("preference", Loc.GetString($"humanoid-profile-editor-preference-{Preference.ToString().ToLower()}"))));
        return profile.Clothing == Preference;
    }
}

/// <summary>
///     Requires the profile to be a certain species
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterSpeciesRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public ProtoId<SpeciesPrototype> Species;

    public override bool IsValid(IPrototype prototype, JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason)
    {
        reason = FormattedMessage.FromMarkup(Loc.GetString("character-species-requirement",
            ("species", Loc.GetString($"species-name-{Species.ToString().ToLower()}"))));
        return profile.Species == Species;
    }
}

/// <summary>
///     Requires the profile to have a certain trait
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterTraitRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public ProtoId<TraitPrototype> Trait;

    public override bool IsValid(IPrototype prototype, JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason)
    {
        reason = FormattedMessage.FromMarkup(Loc.GetString("character-trait-requirement",
            ("trait", Loc.GetString($"trait-{Trait.ToString().ToLower()}-name"))));
        return profile.TraitPreferences.Contains(Trait.ToString());
    }
}

#endregion

#region Jobs

/// <summary>
///     Requires the selected job to be a certain one
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterJobRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public List<ProtoId<JobPrototype>> Jobs;

    public override bool IsValid(IPrototype prototype, JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason)
    {
        // Join localized job names with a comma
        var jobsString = string.Join(", ", Jobs.Select(j => Loc.GetString(prototypeManager.Index(j).Name)));
        // Form the reason message
        jobsString = Loc.GetString("character-job-requirement", ("job", jobsString));

        reason = FormattedMessage.FromMarkup(jobsString);
        return Jobs.Contains(job.ID);
    }
}

/// <summary>
///     Requires the playtime for a department to be within a certain range
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterDepartmentTimeRequirement : CharacterRequirement
{
    [DataField]
    public TimeSpan Min = TimeSpan.MinValue;

    [DataField]
    public TimeSpan Max = TimeSpan.MaxValue;

    [DataField(required: true)]
    public ProtoId<DepartmentPrototype> Department;

    public override bool IsValid(IPrototype prototype, JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason)
    {
        // Disable the requirement if the role timers are disabled
        if (!configManager.GetCVar(CCVars.GameRoleTimers))
        {
            reason = null;
            return !Inverted;
        }

        var department = prototypeManager.Index(Department);

        // Combine all of this department's job playtimes
        var playtime = TimeSpan.Zero;
        foreach (var other in department.Roles)
        {
            var proto = prototypeManager.Index<JobPrototype>(other).PlayTimeTracker;

            playTimes.TryGetValue(proto, out var otherTime);
            playtime += otherTime;
        }

        if (playtime > Max)
        {
            // Show the reason if invalid
            reason = Inverted
                ? null
                : FormattedMessage.FromMarkup(Loc.GetString("character-timer-department-too-high",
                    ("time", playtime.Minutes - Max.Minutes),
                    ("department", Loc.GetString($"department-{department.ID}")),
                    ("departmentColor", department.Color)));
            return false;
        }

        if (playtime < Min)
        {
            // Show the reason if invalid
            reason = Inverted
                ? null
                : FormattedMessage.FromMarkup(Loc.GetString("character-timer-department-insufficient",
                    ("time", Min.Minutes - playtime.Minutes),
                    ("department", Loc.GetString($"department-{department.ID}")),
                    ("departmentColor", department.Color)));
            return false;
        }

        reason = null;
        return true;
    }
}

/// <summary>
///     Requires the player to have a certain amount of overall job time
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterOverallTimeRequirement : CharacterRequirement
{
    [DataField]
    public TimeSpan Min = TimeSpan.MinValue;

    [DataField]
    public TimeSpan Max = TimeSpan.MaxValue;

    public override bool IsValid(IPrototype prototype, JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason)
    {
        // Disable the requirement if the role timers are disabled
        if (!configManager.GetCVar(CCVars.GameRoleTimers))
        {
            reason = null;
            return !Inverted;
        }

        // Get the overall time
        var overallTime = playTimes.GetValueOrDefault(PlayTimeTrackingShared.TrackerOverall);

        if (overallTime > Max)
        {
            // Show the reason if invalid
            reason = Inverted
                ? null
                : FormattedMessage.FromMarkup(Loc.GetString("character-timer-overall-too-high",
                    ("time", overallTime.Minutes - Max.Minutes)));
            return false;
        }

        if (overallTime < Min)
        {
            // Show the reason if invalid
            reason = Inverted
                ? null
                : FormattedMessage.FromMarkup(Loc.GetString("character-timer-overall-insufficient",
                    ("time", Min.Minutes - overallTime.Minutes)));
            return false;
        }

        reason = null;
        return true;
    }
}

/// <summary>
///     Requires the playtime for a tracker to be within a certain range
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterPlaytimeRequirement : CharacterRequirement
{
    [DataField]
    public TimeSpan Min = TimeSpan.MinValue;

    [DataField]
    public TimeSpan Max = TimeSpan.MaxValue;

    [DataField(required: true)]
    public ProtoId<PlayTimeTrackerPrototype> Tracker;

    public override bool IsValid(IPrototype prototype, JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason)
    {
        // Disable the requirement if the role timers are disabled
        if (!configManager.GetCVar(CCVars.GameRoleTimers))
        {
            reason = null;
            return !Inverted;
        }

        // Get SharedJobSystem
        if (!entityManager.EntitySysManager.TryGetEntitySystem(out SharedJobSystem? jobSystem))
        {
            DebugTools.Assert("CharacterRequirements: SharedJobSystem not found");
            reason = null;
            return false;
        }

        // Get the JobPrototype of the Tracker
        var trackerJob = jobSystem.GetJobPrototype(Tracker);

        // Get the primary department of the Tracker
        if (!jobSystem.TryGetPrimaryDepartment(trackerJob, out var department) &&
            !jobSystem.TryGetDepartment(trackerJob, out department))
        {
            DebugTools.Assert($"CharacterRequirements: Department not found for job {trackerJob}");
            reason = null;
            return false;
        }

        // Get the time for the tracker
        playTimes.TryGetValue(Tracker, out var time);
        reason = null;

        if (time > Max)
        {
            // Show the reason if invalid
            reason = Inverted
                ? null
                : FormattedMessage.FromMarkup(Loc.GetString("character-timer-role-too-high",
                    ("time", time.Minutes - Max.Minutes),
                    ("job", trackerJob),
                    ("departmentColor", department.Color)));
            return false;
        }

        if (time < Min)
        {
            // Show the reason if invalid
            reason = Inverted
                ? null
                : FormattedMessage.FromMarkup(Loc.GetString("character-timer-role-insufficient",
                    ("time", Min.Minutes - time.Minutes),
                    ("job", trackerJob),
                    ("departmentColor", department.Color)));
            return false;
        }

        return true;
    }
}

#endregion
