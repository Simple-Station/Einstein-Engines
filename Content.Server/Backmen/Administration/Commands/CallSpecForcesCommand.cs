using Content.Server.Administration;
using Content.Server.Administration.Logs;
using Content.Server.Backmen.SpecForces;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Content.Shared.Database;
using Robust.Shared.Prototypes;
using static System.Int32;

namespace Content.Server.Backmen.Administration.Commands;

[AdminCommand(AdminFlags.Admin)]
public sealed class CallSpecForcesCommand : IConsoleCommand
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly EntityManager _entManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    public string Command => "callspecforces";
    public string Description => "Вызов команды спецсил";
    public string Help => "callspecforces";

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var specForceSystem = _entManager.System<SpecForcesSystem>();

        switch (args.Length)
        {
            case 1:
                if(!specForceSystem.CallOps(args[0],shell.Player != null ? shell.Player.Name : "An administrator"))
                    shell.WriteLine($"Подождите еще {specForceSystem.DelayTime} перед запуском следующих!");
                _adminLogger.Add(
                    LogType.AdminMessage,
                    LogImpact.Extreme,
                    $"Admin {(shell.Player != null ? shell.Player.Name : "An administrator")} called SpecForceTeam {args[0]}.");
                break;
            default:
                shell.WriteLine(Loc.GetString("shell-wrong-arguments-number"));
                break;
        }
    }
    public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        // Index all SpecForceTeams prototypes and write them down in completion result.
        return args.Length switch
        {
            1 => CompletionResult.FromHintOptions(
                CompletionHelper.PrototypeIDs<SpecForceTeamPrototype>(true, _prototypes),
                "Тип вызова"),
            _ => CompletionResult.Empty
        };
    }
}
