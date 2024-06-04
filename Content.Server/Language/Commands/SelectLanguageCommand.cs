using System.Linq;
using Content.Shared.Administration;
using Robust.Shared.Console;
using Robust.Shared.Enums;

namespace Content.Server.Language.Commands;

[AnyCommand]
public sealed class SelectLanguageCommand : IConsoleCommand
{
    public string Command => "languageselect";
    public string Description => Loc.GetString("command-language-select-desc");
    public string Help => Loc.GetString("command-language-select-help", ("command", Command));

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is not { } player)
        {
            shell.WriteError(Loc.GetString("shell-cannot-run-command-from-server"));
            return;
        }

        if (player.Status != SessionStatus.InGame)
            return;

        if (player.AttachedEntity is not { } playerEntity)
        {
            shell.WriteError(Loc.GetString("shell-must-be-attached-to-entity"));
            return;
        }

        if (args.Length < 1)
            return;

        var languageId = args[0];

        var languages = IoCManager.Resolve<IEntitySystemManager>().GetEntitySystem<LanguageSystem>();

        var language = languages.GetLanguagePrototype(languageId);
        if (language == null || !languages.CanSpeak(playerEntity, language.ID))
        {
            shell.WriteError($"Language {languageId} is invalid or you cannot speak it!");
            return;
        }

        languages.SetLanguage(playerEntity, language.ID);
    }
}
