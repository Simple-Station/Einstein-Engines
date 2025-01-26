using System.Linq;
using Content.Server.Players.PlayTimeTracking;
using Content.Shared.Administration;
using Content.Shared.Customization.Systems;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;


namespace Content.Server.Administration.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class PlayTimeUnlockCommands : IConsoleCommand
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly PlayTimeTrackingManager _playTimeTracking = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    public string Command => "playtime_unlock";
    public string Description => Loc.GetString("cmd-playtime_unlock-desc");
    public string Help => Loc.GetString("cmd-playtime_unlock-help", ("command", Command));

    private Dictionary<string, string> _departmentToTrackers = new();

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (_departmentToTrackers.Count == 0)
            PopulateDepartmentConversions();

        if (args.Length != 2)
        {
            shell.WriteError(Loc.GetString("cmd-playtime_addoverall-error-args"));
            return;
        }

        if (!_playerManager.TryGetSessionByUsername(args[0], out var player))
        {
            shell.WriteError(Loc.GetString("parse-session-fail", ("username", args[0])));
            return;
        }

        var jobName = args[1];
        var jobExists = _prototypeManager.TryIndex<JobPrototype>(jobName, out var job);

        if (!jobExists)
        {
            shell.WriteError(Loc.GetString("cmd-playtime_unlock-error-job", ("invalidJob", jobName)));
            return;
        }

        if (job == null || job.Requirements == null)
        {
            shell.WriteError(Loc.GetString("cmd-playtime_unlock-error-no-requirements"));
            return;
        }

        var jobPlaytimeRequirements = job.Requirements
            .Where(r => r is CharacterPlaytimeRequirement)
            .Cast<CharacterPlaytimeRequirement>()
            .ToList();

        var jobDepartmentRequirements = job.Requirements
            .Where(r => r is CharacterDepartmentTimeRequirement)
            .Cast<CharacterDepartmentTimeRequirement>()
            .ToList();

        if (!jobPlaytimeRequirements.Any() && !jobDepartmentRequirements.Any())
        {
            shell.WriteError(Loc.GetString("cmd-playtime_unlock-error-no-requirements"));
            return;
        }

        foreach (var jobPlaytimeRequirement in jobPlaytimeRequirements)
            _playTimeTracking.AddTimeToTracker(player, jobPlaytimeRequirement.Tracker, jobPlaytimeRequirement.Min);

        foreach (var jobDepartmentRequirement in jobDepartmentRequirements)
        {
            if (!_departmentToTrackers.TryGetValue(jobDepartmentRequirement.Department, out var jobId))
                continue;

            var exists = _prototypeManager.TryIndex<JobPrototype>(jobId, out var jobPrototype);

            if (!exists)
                continue;

            _playTimeTracking.AddTimeToTracker(player, jobPrototype!.PlayTimeTracker, jobDepartmentRequirement.Min);
        }

        shell.WriteLine(Loc.GetString("shell-command-success"));
    }

    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(
                CompletionHelper.SessionNames(players: _playerManager),
                Loc.GetString("cmd-playtime_unlock-arg-user"));
        }

        if (args.Length == 2)
        {
            return CompletionResult.FromHintOptions(
                CompletionHelper.PrototypeIDs<JobPrototype>(),
                Loc.GetString("cmd-playtime_unlock-arg-job"));
        }

        return CompletionResult.Empty;
    }

    private void PopulateDepartmentConversions()
    {
        var allDepartments = _prototypeManager.EnumeratePrototypes<DepartmentPrototype>()
            .ToList();

        foreach (var department in allDepartments)
        {
            if (_departmentToTrackers.ContainsKey(department.ID))
                continue;

            if (department.Roles.Count == 0)
                continue;

            _departmentToTrackers.Add(department.ID, department.Roles[0]);
        }
    }
}
