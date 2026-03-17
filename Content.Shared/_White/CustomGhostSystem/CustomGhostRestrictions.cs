using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Roles;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Content.Shared._White.CustomGhostSystem;


// omitting CustomGhost prefix for convenience when prototyping
[DataDefinition]
public sealed partial class CkeyRestriction : CustomGhostRestriction
{
    [DataField(required: true)]
    public List<string> Ckey = new();

    public override bool HideOnFail => true;

    public override bool CanUse(ICommonSession player, [NotNullWhen(false)] out string? failReason)
    {
        failReason = null;
        var name = player.Name.ToLower();
        if (player.Name.StartsWith("localhost@"))
            name = name.Substring(10); // what's the proper way of doing this?

        foreach(var testCkey in Ckey)
            if(testCkey.ToLower() == name) // I'd do a .Contains() here, but i want this check to be case independent, and the StringComparer.OrdinalIgnoreCase
                return true;              // is not allowed by the sandbox because robusttoolbox maintainers are mentally disabled

        failReason = Loc.GetString("custom-ghost-fail-exclusive-ghost");
        return false;
    }
}

// todo refactor this copypaste bullshit

[DataDefinition]
public sealed partial class PlaytimeServerRestriction : CustomGhostRestriction
{
    private static ISharedPlaytimeManager? _playtime = null;

    [DataField(required: true)]
    public float HoursPlaytime;

    public override bool CanUse(ICommonSession player, [NotNullWhen(false)] out string? failReason)
    {
        _playtime ??= IoCManager.Resolve<ISharedPlaytimeManager>();

        failReason = null;
        var playtimes = _playtime.GetPlayTimes(player);

        double jobPlaytime = 0;
        if (playtimes.TryGetValue("Overall", out var time))
            jobPlaytime += time.TotalHours;

        if (jobPlaytime < HoursPlaytime)
        {
            failReason = Loc.GetString("custom-ghost-fail-server-insufficient-playtime",
                    ("requiredHours", MathF.Round(HoursPlaytime)),
                    ("requiredMinutes", MathF.Round(HoursPlaytime % 1 * 60)),
                    ("playtimeHours", Math.Round(jobPlaytime)),
                    ("playtimeMinutes", Math.Round(jobPlaytime % 1 * 60))
            );
            return false;
        }

        return true;
    }
}


[DataDefinition]
public sealed partial class PlaytimeJobRestriction : CustomGhostRestriction
{
    private static ISharedPlaytimeManager? _playtime = null;
    private static IPrototypeManager? _proto = null;

    [DataField(required: true)]
    public string Job = string.Empty;

    [DataField(required: true)]
    public float HoursPlaytime;

    public override bool CanUse(ICommonSession player, [NotNullWhen(false)] out string? failReason)
    {
        _playtime ??= IoCManager.Resolve<ISharedPlaytimeManager>();
        _proto ??= IoCManager.Resolve<IPrototypeManager>();

        failReason = null;
        var playtimes = _playtime.GetPlayTimes(player);

        double jobPlaytime = 0;
        var jobProto = _proto.Index<JobPrototype>(Job);
        if (playtimes.TryGetValue(jobProto.PlayTimeTracker, out var time))
            jobPlaytime += time.TotalHours;

        if (jobPlaytime < HoursPlaytime)
        {
            failReason = Loc.GetString("custom-ghost-fail-job-insufficient-playtime",
                    ("job", Loc.GetString(jobProto.Name)),
                    ("requiredHours", MathF.Round(HoursPlaytime)),
                    ("requiredMinutes", MathF.Round(HoursPlaytime % 1 * 60)),
                    ("playtimeHours", Math.Round(jobPlaytime)),
                    ("playtimeMinutes", Math.Round(jobPlaytime % 1 * 60))
            );
            return false;
        }

        return true;
    }
}


[DataDefinition]
public sealed partial class PlaytimeDepartmentRestriction : CustomGhostRestriction
{
    private static ISharedPlaytimeManager? _playtime = null;
    private static IConfigurationManager? _cfg = null;
    private static IPrototypeManager? _proto = null;

    [DataField(required: true)]
    public ProtoId<DepartmentPrototype> Department = string.Empty;

    [DataField(required: true)]
    public float HoursPlaytime;

    public override bool CanUse(ICommonSession player, [NotNullWhen(false)] out string? failReason)
    {
        _playtime ??= IoCManager.Resolve<ISharedPlaytimeManager>();
        _proto ??= IoCManager.Resolve<IPrototypeManager>();

        failReason = null;
        var playtimes = _playtime.GetPlayTimes(player);

        double departmentPlaytime = 0;
        var departmentProto = _proto.Index<DepartmentPrototype>(Department);
        var departmentJobs = departmentProto.Roles;
        foreach (var job in departmentJobs)
        {
            var jobProto = _proto.Index<JobPrototype>(job);
            if (playtimes.TryGetValue(jobProto.PlayTimeTracker, out var time))
                departmentPlaytime += time.TotalHours;
        }
        if (departmentPlaytime < HoursPlaytime)
        {
            failReason = Loc.GetString("custom-ghost-fail-department-insufficient-playtime",
                    ("department", Loc.GetString(departmentProto.Name)),
                    ("requiredHours", MathF.Round(HoursPlaytime)),
                    ("requiredMinutes", MathF.Round(HoursPlaytime % 1 * 60)),
                    ("playtimeHours", Math.Round(departmentPlaytime)),
                    ("playtimeMinutes", Math.Round(departmentPlaytime % 1 * 60))
            );
            return false;
        }

        return true;
    }
}
