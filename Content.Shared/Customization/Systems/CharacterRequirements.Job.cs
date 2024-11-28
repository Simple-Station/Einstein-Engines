using System.Linq;
using Content.Shared.CCVar;
using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Preferences;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using JetBrains.Annotations;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared.Customization.Systems;


/// <summary>
///     Requires the selected job to be one of the specified jobs
/// </summary>
[UsedImplicitly]
[Serializable, NetSerializable]
public sealed partial class CharacterJobRequirement : CharacterRequirement
{
    [DataField(required: true)]
    public List<ProtoId<JobPrototype>> Jobs;

    public override bool IsValid(JobPrototype job,
        HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        bool whitelisted,
        IPrototype prototype,
        IEntityManager entityManager,
        IPrototypeManager prototypeManager,
        IConfigurationManager configManager,
        out string? reason,
        int depth = 0)
    {
        var jobs = new List<string>();
        var depts = prototypeManager.EnumeratePrototypes<DepartmentPrototype>();

        // Get the job names and department colors
        foreach (var j in Jobs)
        {
            var jobProto = prototypeManager.Index(j);
            var color = Color.LightBlue;

            foreach (var dept in depts.ToList().OrderBy(d => Loc.GetString($"department-{d.ID}")))
            {
                if (!dept.Roles.Contains(j))
                    continue;

                color = dept.Color;
                break;
            }

            jobs.Add($"[color={color.ToHex()}]{Loc.GetString(jobProto.Name)}[/color]");
        }

        // Join the job names
        var jobsString = Loc.GetString("character-job-requirement",
            ("inverted", Inverted), ("jobs", string.Join(", ", jobs)));

        reason = jobsString;
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

    public override bool IsValid(JobPrototype job,
        HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        bool whitelisted,
        IPrototype prototype,
        IEntityManager entityManager,
        IPrototypeManager prototypeManager,
        IConfigurationManager configManager,
        out string? reason,
        int depth = 0)
    {
        var departments = new List<string>();

        // Get the department names and colors
        foreach (var d in Departments)
        {
            var deptProto = prototypeManager.Index(d);
            var color = deptProto.Color;

            departments.Add($"[color={color.ToHex()}]{Loc.GetString($"department-{deptProto.ID}")}[/color]");
        }

        // Join the department names
        var departmentsString = Loc.GetString("character-department-requirement",
            ("inverted", Inverted), ("departments", string.Join(", ", departments)));

        reason = departmentsString;
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

    public override bool IsValid(JobPrototype job,
        HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        bool whitelisted,
        IPrototype prototype,
        IEntityManager entityManager,
        IPrototypeManager prototypeManager,
        IConfigurationManager configManager,
        out string? reason,
        int depth = 0)
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
                : Loc.GetString("character-timer-department-too-high",
                    ("time", playtime.TotalMinutes - Max.TotalMinutes),
                    ("department", Loc.GetString($"department-{department.ID}")),
                    ("departmentColor", department.Color));
            return false;
        }

        if (playtime < Min)
        {
            // Show the reason if invalid
            reason = Inverted
                ? null
                : Loc.GetString("character-timer-department-insufficient",
                    ("time", Min.TotalMinutes - playtime.TotalMinutes),
                    ("department", Loc.GetString($"department-{department.ID}")),
                    ("departmentColor", department.Color));
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

    public override bool IsValid(JobPrototype job,
        HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        bool whitelisted,
        IPrototype prototype,
        IEntityManager entityManager,
        IPrototypeManager prototypeManager,
        IConfigurationManager configManager,
        out string? reason,
        int depth = 0)
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
                : Loc.GetString("character-timer-overall-too-high",
                    ("time", overallTime.TotalMinutes - Max.TotalMinutes));
            return false;
        }

        if (overallTime < Min)
        {
            // Show the reason if invalid
            reason = Inverted
                ? null
                : Loc.GetString("character-timer-overall-insufficient",
                    ("time", Min.TotalMinutes - overallTime.TotalMinutes));
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

    public override bool IsValid(JobPrototype job,
        HumanoidCharacterProfile profile,
        Dictionary<string, TimeSpan> playTimes,
        bool whitelisted,
        IPrototype prototype,
        IEntityManager entityManager,
        IPrototypeManager prototypeManager,
        IConfigurationManager configManager,
        out string? reason,
        int depth = 0)
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
        var jobStr = prototypeManager.Index<JobPrototype>(trackerJob).LocalizedName;

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
                : Loc.GetString("character-timer-role-too-high",
                    ("time", time.TotalMinutes - Max.TotalMinutes),
                    ("job", jobStr),
                    ("departmentColor", department.Color));
            return false;
        }

        if (time < Min)
        {
            // Show the reason if invalid
            reason = Inverted
                ? null
                : Loc.GetString("character-timer-role-insufficient",
                    ("time", Min.TotalMinutes - time.TotalMinutes),
                    ("job", jobStr),
                    ("departmentColor", department.Color));
            return false;
        }

        return true;
    }
}
