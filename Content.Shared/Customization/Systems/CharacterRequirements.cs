using System.Linq;
using Content.Shared.CCVar;
using Content.Shared.Clothing.Loadouts.Prototypes;
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
            ("inverted", Inverted), ("min", Min), ("max", Max)));
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
            ("inverted", Inverted),
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
            ("inverted", Inverted),
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
            ("inverted", Inverted),
            ("species", Loc.GetString($"species-name-{Species.ToString().ToLower()}"))));
        return profile.Species == Species;
    }
}

/// <summary>
///     Requires the profile to have one of the specified traits
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterTraitRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public List<ProtoId<TraitPrototype>> Traits;

    public override bool IsValid(IPrototype prototype, JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason)
    {
        reason = FormattedMessage.FromMarkup(Loc.GetString("character-trait-requirement", ("inverted", Inverted),
            ("traits", string.Join(", ", Traits.Select(t => Loc.GetString($"trait-name-{t}"))))));

        return Traits.Any(t => profile.TraitPreferences.Contains(t.ToString()));
    }
}

/// <summary>
///     Requires the profile to have one of the specified loadouts
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterLoadoutRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public List<ProtoId<LoadoutPrototype>> Loadouts;

    public override bool IsValid(IPrototype prototype, JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes, IEntityManager entityManager, IPrototypeManager prototypeManager,
        IConfigurationManager configManager, out FormattedMessage? reason)
    {
        reason = FormattedMessage.FromMarkup(Loc.GetString("character-loadout-requirement", ("inverted", Inverted),
            ("loadouts", string.Join(", ", Loadouts.Select(l => Loc.GetString($"loadout-{l}"))))));

        return Loadouts.Any(l => profile.LoadoutPreferences.Contains(l.ToString()));
    }
}

#endregion

#region Jobs

/// <summary>
///     Requires the selected job to be one of the specified jobs
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
        var jobs = new List<FormattedMessage>();

        // Get the job names and department colors
        foreach (var j in Jobs)
        {
            var jobProto = prototypeManager.Index(j);
            var color = Color.LightBlue;

            foreach (var dept in prototypeManager.EnumeratePrototypes<DepartmentPrototype>()
                .OrderBy(d => Loc.GetString($"department-{d.ID}")))
            {
                if (!dept.Roles.Contains(j))
                    continue;

                color = dept.Color;
                break;
            }

            jobs.Add(FormattedMessage.FromMarkup($"[color={color.ToHex()}]{Loc.GetString(jobProto.Name)}[/color]"));
        }

        // Join the job names
        var jobsList = string.Join(", ", jobs.Select(j => j.ToMarkup()));
        var jobsString = Loc.GetString("character-job-requirement",
            ("inverted", Inverted), ("jobs", jobsList));

        reason = FormattedMessage.FromMarkup(jobsString);
        return Jobs.Contains(job.ID);
    }
}

/// <summary>
///     Requires the selected job to be in one of the specified departments
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterDepartmentRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public List<ProtoId<DepartmentPrototype>> Departments;

    public override bool IsValid(IPrototype prototype, JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason)
    {
        var departments = new List<FormattedMessage>();

        // Get the department names and colors
        foreach (var d in Departments)
        {
            var deptProto = prototypeManager.Index(d);
            var color = deptProto.Color;

            departments.Add(FormattedMessage.FromMarkup($"[color={color.ToHex()}]{Loc.GetString($"department-{deptProto.ID}")}[/color]"));
        }

        // Join the department names
        var departmentsList = string.Join(", ", departments.Select(d => d.ToMarkup()));
        var departmentsString = Loc.GetString("character-department-requirement",
            ("inverted", Inverted), ("departments", departmentsList));

        reason = FormattedMessage.FromMarkup(departmentsString);
        return Departments.Any(d => prototypeManager.Index(d).Roles.Contains(job.ID));
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
        var time = playTimes.GetValueOrDefault(Tracker);
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

#region Prototype Groups

/// <summary>
///     Requires the profile to not have any of the specified traits
/// </summary>
/// <remarks>
///     Only works if you put this prototype in the denied prototypes' requirements too.
///     Can't be inverted, use <see cref="CharacterTraitRequirement"/>
/// </remarks>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class TraitGroupExclusionRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public List<ProtoId<TraitPrototype>> Prototypes;

    public override bool IsValid(IPrototype prototype, JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason)
    {
        var invalid = profile.TraitPreferences.Any(t => Prototypes.Contains(t));

        reason = FormattedMessage.FromMarkup(Loc.GetString("character-trait-group-exclusion-requirement",
            ("traits", string.Join(", ", Prototypes.Select(t => Loc.GetString($"trait-name-{t}"))))));

        return Inverted ? invalid : !invalid;
    }
}

/// <summary>
///     Requires the profile to not have any of the specified loadouts
/// </summary>
/// <remarks>
///     Only works if you put this prototype in the denied prototypes' requirements too.
///     Can't be inverted, use <see cref="CharacterLoadoutRequirement"/>
/// </remarks>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class LoadoutGroupExclusionRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public List<ProtoId<LoadoutPrototype>> Prototypes;

    public override bool IsValid(IPrototype prototype, JobPrototype job, HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        IEntityManager entityManager, IPrototypeManager prototypeManager, IConfigurationManager configManager,
        out FormattedMessage? reason)
    {
        var invalid = profile.LoadoutPreferences.Any(l => Prototypes.Contains(l));

        reason = FormattedMessage.FromMarkup(Loc.GetString("character-loadout-group-exclusion-requirement",
            ("loadouts", string.Join(", ", Prototypes.Select(l => Loc.GetString($"loadout-{l}"))))));

        return Inverted ? invalid : !invalid;
    }
}

#endregion
