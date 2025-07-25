using System.Linq;
using Content.Server.Administration;
using Content.Shared.Administration;
using Content.Shared.StationGoal;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;

namespace Content.Server.StationGoal
{
    [AdminCommand(AdminFlags.Fun)]
    public sealed class StationGoalCommand : IConsoleCommand
    {
        public string Command => "sendstationgoal";
        public string Description => Loc.GetString("send-station-goal-command-description");
        public string Help => Loc.GetString("send-station-goal-command-help-text", ("command", Command));

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 1)
            {
                shell.WriteError(Loc.GetString("shell-need-exactly-one-argument"));
                return;
            }

            var protoId = args[0];
            var prototypeManager = IoCManager.Resolve<IPrototypeManager>();
            if (!prototypeManager.TryIndex<StationGoalPrototype>(protoId, out var proto))
            {
                shell.WriteError(Loc.GetString("send-station-goal-command-error-no-goal-proto", ("id", protoId)));
                return;
            }

            var stationGoalPaper = IoCManager.Resolve<IEntityManager>().System<StationGoalPaperSystem>();
            if (!stationGoalPaper.SendStationGoal(proto))
            {
                shell.WriteError(Loc.GetString("send-station-goal-command-error-couldnt-fax"));
                return;
            }
        }

        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            if (args.Length == 1)
            {
                var options = IoCManager.Resolve<IPrototypeManager>()
                    .EnumeratePrototypes<StationGoalPrototype>()
                    .OrderBy(p => p.ID)
                    .Select(p => new CompletionOption(p.ID));

                return CompletionResult.FromHintOptions(options, Loc.GetString("send-station-goal-command-arg-id"));
            }

            return CompletionResult.Empty;
        }
    }
}
