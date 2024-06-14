using System.Linq;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Enums;

namespace Content.Server.Language.Commands;

[AnyCommand]
public sealed class ListLanguagesCommand : IConsoleCommand
{
    public string Command => "languagelist";
    public string Description => Loc.GetString("command-list-langs-desc");
    public string Help => Loc.GetString("command-list-langs-help", ("command", Command));

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is not { } player)
        {
            shell.WriteError(Loc.GetString("shell-cannot-run-command-from-server"));
            return;
        }

        if (player.Status != SessionStatus.InGame)
            return;

        if (player.AttachedEntity is not {} playerEntity)
        {
            shell.WriteError(Loc.GetString("shell-must-be-attached-to-entity"));
            return;
        }

        var languages = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<LanguageSystem>();

        var (spokenLangs, knownLangs) = languages.GetAllLanguages(playerEntity);

        shell.WriteLine("Spoken:\n" + string.Join("\n", spokenLangs));
        shell.WriteLine("Understood:\n" + string.Join("\n", knownLangs));
    }
}
