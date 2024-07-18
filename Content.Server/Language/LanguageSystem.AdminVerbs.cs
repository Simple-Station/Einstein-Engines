using System.Linq;
using Content.Server.Administration;
using Content.Server.Administration.Managers;
using Content.Server.Administration.Systems;
using Content.Server.Chat.Managers;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Language;
using Content.Shared.Language.Components;
using Content.Shared.Language.Components.Translators;
using Content.Shared.Verbs;
using Robust.Shared.Player;
using Robust.Shared.Utility;
using static Content.Server.Administration.Systems.AdminVerbSystem.TricksVerbPriorities;

namespace Content.Server.Language;

public sealed partial class LanguageSystem
{
    [Dependency] private readonly IAdminManager _adminManager = default!;
    [Dependency] private readonly IChatManager _chatManager = default!;
    [Dependency] private readonly QuickDialogSystem _quickDialog = default!;

    private void InitializeAdmin()
    {
        SubscribeLocalEvent<GetVerbsEvent<Verb>>(OnGetVerbs);
    }

    private void OnGetVerbs(GetVerbsEvent<Verb> args)
    {
        if (!EntityManager.TryGetComponent(args.User, out ActorComponent? actor))
            return;

        var player = actor.PlayerSession;
        if (!_adminManager.HasAdminFlag(player, AdminFlags.Admin))
            return;

        // Remove/add universal speak comp
        if (HasComp<UniversalLanguageSpeakerComponent>(args.Target))
        {
            Verb removeUniversal = new()
            {
                Text = "Remove Universal",
                Message = Loc.GetString("admin-trick-remove-universal-description"),
                Category = VerbCategory.Tricks,
                Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/AdminActions/universal-remove.png")),
                Act = () =>
                {
                    EnsureComp<LanguageSpeakerComponent>(args.Target);
                    RemComp<UniversalLanguageSpeakerComponent>(args.Target);
                    UpdateEntityLanguages(args.Target);
                },
                Impact = LogImpact.Medium,
                Priority = (int) Languages
            };
            args.Verbs.Add(removeUniversal);
        }
        else if (HasComp<LanguageSpeakerComponent>(args.Target))
        {
            Verb addUniversal = new()
            {
                Text = "Add Universal",
                Message = Loc.GetString("admin-trick-add-universal-description"),
                Category = VerbCategory.Tricks,
                Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/AdminActions/universal-add.png")),
                Act = () =>
                {
                    EnsureComp<UniversalLanguageSpeakerComponent>(args.Target);
                    UpdateEntityLanguages(args.Target);
                },
                Impact = LogImpact.Medium,
                Priority = (int) Languages
            };
            args.Verbs.Add(addUniversal);
        }

        // Edit entity languages
        if (TryComp<LanguageSpeakerComponent>(args.Target, out var speaker))
        {
            Verb addLanguage = new()
            {
                Text = "Edit Languages",
                Message = Loc.GetString("admin-trick-add-language-description"),
                Category = VerbCategory.Tricks,
                Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/AdminActions/language-add.png")),
                Act = () =>
                {
                    _quickDialog.OpenDialog<string, bool, bool>(player, "Add Language", "Language ID", "Can understand", "Can speak", (id, canUnderstand, canSpeak) =>
                    {
                        if (ValidateLanguageOrInformUser(player, id))
                            AddLanguage(args.Target, id, canSpeak, canUnderstand, speaker: speaker);
                    });
                },
                Impact = LogImpact.Medium,
                Priority = (int) Languages
            };

            Verb removeLanguage = new()
            {
                Text = "Remove Language",
                Message = Loc.GetString("admin-trick-remove-language-description"),
                Category = VerbCategory.Tricks,
                Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/AdminActions/language-remove.png")),
                Act = () =>
                {
                    _quickDialog.OpenDialog<string, bool, bool>(player, "Remove Language", "Language ID", "Remove understood", "Remove spoken", (id, understood, spoken) =>
                    {
                        if (ValidateLanguageOrInformUser(player, id))
                            RemoveLanguage(args.Target, id, spoken, understood, speaker: speaker);
                    });
                },
                Impact = LogImpact.Medium,
                Priority = (int) Languages
            };

            args.Verbs.Add(removeLanguage);
            args.Verbs.Add(addLanguage);
        }

        BaseTranslatorComponent? translator = null;
        if (TryComp<HandheldTranslatorComponent>(args.Target, out var handheldComp))
            translator = handheldComp;
        else if (TryComp<TranslatorImplantComponent>(args.Target, out var implantComp))
            translator = implantComp;
        else if (TryComp<IntrinsicTranslatorComponent>(args.Target, out var intrinsicComp))
            translator = intrinsicComp;

        // Edit translator languages
        if (translator != null)
        {
            Verb editTranslator = new()
            {
                Text = "Edit translator",
                Message = Loc.GetString("admin-trick-edit-translator-description"),
                Category = VerbCategory.Tricks,
                Icon = new SpriteSpecifier.Texture(new("/Textures/Interface/AdminActions/translator-edit.png")),
                Act = () =>
                {
                    _quickDialog.OpenDialog<LongString, LongString, LongString>(player,
                        "Edit translator (separate languages with comma)",
                        "Understood:",
                        "Spoken:",
                        "Required:",
                        (understood, spoken, required) =>
                        {
                            if (!SplitAndValidateLanguages(understood, ',', player, out var understoodLanguages)
                                || !SplitAndValidateLanguages(spoken, ',', player, out var spokenLanguages)
                                || !SplitAndValidateLanguages(required, ',', player, out var requiredLanguages))
                                return;

                            // This probably won't cause issues with handheld translators, but may be problematic if admeme decides to edit an already implanted translator.
                            translator.UnderstoodLanguages = understoodLanguages;
                            translator.SpokenLanguages = spokenLanguages;
                            translator.RequiredLanguages = requiredLanguages;
                        });
                },
                Impact = LogImpact.Medium,
                Priority = (int) Languages
            };

            args.Verbs.Add(editTranslator);
        }
    }

    /// <summary>
    ///     Ensure a language prototype with the given id exists, and send a message to the user if it doesn't.
    /// </summary>
    private bool ValidateLanguageOrInformUser(ICommonSession session, string languageId)
    {
        if (GetLanguagePrototype(languageId) == null)
        {
            _chatManager.DispatchServerMessage(session, Loc.GetString("admin-trick-error-no-such-language", ("id", languageId)), true);
            return false;
        }

        return true;
    }

    /// <summary>
    ///     Split a string containing a list of languages, validate each one of them, and inform the user if any of them is invalid.
    /// </summary>
    private bool SplitAndValidateLanguages(string languageStr, char separator, ICommonSession session, out List<string> languageList)
    {
        languageList = languageStr
            .Split(separator)
            .Select(it => it.Trim())
            .Where(it => it.Length > 0)
            .ToList();
        return languageList.All(it => ValidateLanguageOrInformUser(session, it));
    }
}
