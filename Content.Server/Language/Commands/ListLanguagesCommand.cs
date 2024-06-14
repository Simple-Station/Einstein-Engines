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
        var currentLang = languages.GetLanguage(playerEntity).ID;

        shell.WriteLine(Loc.GetString("command-language-spoken"));
        for (int i = 0; i < spokenLangs.Count; i++)
        {
            var lang = spokenLangs[i];
            shell.WriteLine(lang == currentLang
                ? Loc.GetString("command-language-current-entry", ("id", i + 1), ("language", lang), ("name", LanguageName(lang)))
                : Loc.GetString("command-language-entry", ("id", i + 1), ("language", lang), ("name", LanguageName(lang))));
        }

        shell.WriteLine(Loc.GetString("command-language-understood"));
        for (int i = 0; i < knownLangs.Count; i++)
        {
            var lang = knownLangs[i];
            shell.WriteLine(Loc.GetString("command-language-entry", ("id", i + 1), ("language", lang), ("name", LanguageName(lang))));
        }
    }

    private string LanguageName(string id)
    {
        return Loc.GetString($"language-{id}-name");
    }
}
