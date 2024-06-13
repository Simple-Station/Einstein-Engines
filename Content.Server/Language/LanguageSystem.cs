using System.Linq;
using System.Text;
using Content.Server.GameTicking.Events;
using Content.Shared.Language;
using Content.Shared.Language.Events;
using Content.Shared.Language.Systems;
using Robust.Shared.Random;
using UniversalLanguageSpeakerComponent = Content.Shared.Language.Components.UniversalLanguageSpeakerComponent;

namespace Content.Server.Language;

public sealed partial class LanguageSystem : SharedLanguageSystem
{
    // Static and re-used event instances used to minimize memory allocations during language processing, which can happen many times per tick.
    // These are used in the method GetLanguages and returned from it. They should never be mutated outside of that method or returned outside this system.
    private readonly DetermineEntityLanguagesEvent
        _determineLanguagesEvent = new(string.Empty, new(), new()),
        _universalLanguagesEvent = new(UniversalPrototype, [UniversalPrototype], [UniversalPrototype]); // Returned for universal speakers only



    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<LanguagesSetMessage>(OnClientSetLanguage);
        SubscribeLocalEvent<LanguageSpeakerComponent, ComponentInit>(OnInitLanguageSpeaker);

        InitializeNet();
    }


    #region public api
    /// <summary>
    ///   Obfuscate a message using an entity's default language.
    /// </summary>
    public string ObfuscateSpeech(EntityUid source, string message)
    {
        var language = GetLanguage(source);
        return ObfuscateSpeech(message, language);
    }

    public bool CanUnderstand(EntityUid listener, LanguagePrototype language, LanguageSpeakerComponent? listenerLanguageComp = null)
    {
        if (language.ID == UniversalPrototype || HasComp<UniversalLanguageSpeakerComponent>(listener))
            return true;

        var listenerLanguages = GetLanguages(listener, listenerLanguageComp)?.UnderstoodLanguages;

        return listenerLanguages?.Contains(language.ID, StringComparer.Ordinal) ?? false;
    }

    public bool CanSpeak(EntityUid speaker, string language, LanguageSpeakerComponent? speakerComp = null)
    {
        if (HasComp<UniversalLanguageSpeakerComponent>(speaker))
            return true;

        var langs = GetLanguages(speaker, speakerComp)?.UnderstoodLanguages;
        return langs?.Contains(language, StringComparer.Ordinal) ?? false;
    }

    /// <summary>
    ///     Returns the current language of the given entity.
    ///     Assumes Universal if not specified.
    /// </summary>
    public LanguagePrototype GetLanguage(EntityUid speaker, LanguageSpeakerComponent? languageComp = null)
    {
        var id = GetLanguages(speaker, languageComp)?.CurrentLanguage;
        if (id == null)
            return Universal; // Fallback

        _prototype.TryIndex(id, out LanguagePrototype? proto);

        return proto ?? Universal;
    }

    public void SetLanguage(EntityUid speaker, string language, LanguageSpeakerComponent? languageComp = null)
    {
        if (!CanSpeak(speaker, language) || HasComp<UniversalLanguageSpeakerComponent>(speaker))
            return;

        if (languageComp == null && !TryComp(speaker, out languageComp))
            return;

        if (languageComp.CurrentLanguage == language)
            return;

        languageComp.CurrentLanguage = language;

        RaiseLocalEvent(speaker, new LanguagesUpdateEvent(), true);
    }

    /// <summary>
    ///   Adds a new language to the lists of understood and/or spoken languages of the given component.
    /// </summary>
    public void AddLanguage(LanguageSpeakerComponent comp, string language, bool addSpoken = true, bool addUnderstood = true)
    {
        if (addSpoken && !comp.SpokenLanguages.Contains(language))
            comp.SpokenLanguages.Add(language);

        if (addUnderstood && !comp.UnderstoodLanguages.Contains(language))
            comp.UnderstoodLanguages.Add(language);

        RaiseLocalEvent(comp.Owner, new LanguagesUpdateEvent(), true);
    }

    public (List<string> spoken, List<string> understood) GetAllLanguages(EntityUid speaker)
    {
        var languages = GetLanguages(speaker);
        // The lists need to be copied because the internal ones are re-used for performance reasons.
        return (new List<string>(languages.SpokenLanguages), new List<string>(languages.UnderstoodLanguages));
    }

    /// <summary>
    ///   Ensures the given entity has a valid language as its current language.
    ///   If not, sets it to the first entry of its SpokenLanguages list, or universal if it's empty.
    /// </summary>
    public void EnsureValidLanguage(EntityUid entity, LanguageSpeakerComponent? comp = null)
    {
        if (comp == null && !TryComp(entity, out comp))
            return;

        var langs = GetLanguages(entity, comp);
        if (!langs.SpokenLanguages.Contains(comp!.CurrentLanguage, StringComparer.Ordinal))
        {
            comp.CurrentLanguage = langs.SpokenLanguages.FirstOrDefault(UniversalPrototype);
            RaiseLocalEvent(comp.Owner, new LanguagesUpdateEvent(), true);
        }
    }
    #endregion

    #region event handling
    private void OnInitLanguageSpeaker(EntityUid uid, LanguageSpeakerComponent component, ComponentInit args)
    {
        if (string.IsNullOrEmpty(component.CurrentLanguage))
            component.CurrentLanguage = component.SpokenLanguages.FirstOrDefault(UniversalPrototype);
    }
    #endregion

    #region internal api - misc
    /// <summary>
    ///   Dynamically resolves the current language of the entity and the list of all languages it speaks.
    ///
    ///   If the entity is not a language speaker, or is a universal language speaker, then it's assumed to speak Universal,
    ///   aka all languages at once and none at the same time.
    /// </summary>
    /// <remarks>
    ///   The returned event is reused and thus must not be held as a reference anywhere but inside the caller function.
    /// </remarks>
    private DetermineEntityLanguagesEvent GetLanguages(EntityUid speaker, LanguageSpeakerComponent? comp = null)
    {
        // This is a shortcut for ghosts and entities that should not speak normally (admemes)
        if (HasComp<UniversalLanguageSpeakerComponent>(speaker) || !TryComp(speaker, out comp))
            return _universalLanguagesEvent;

        var ev = _determineLanguagesEvent;
        ev.SpokenLanguages.Clear();
        ev.UnderstoodLanguages.Clear();

        ev.CurrentLanguage = comp.CurrentLanguage;
        ev.SpokenLanguages.AddRange(comp.SpokenLanguages);
        ev.UnderstoodLanguages.AddRange(comp.UnderstoodLanguages);

        RaiseLocalEvent(speaker, ev, true);

        if (ev.CurrentLanguage.Length == 0)
            ev.CurrentLanguage = !string.IsNullOrEmpty(comp.CurrentLanguage) ? comp.CurrentLanguage : UniversalPrototype; // Fall back to account for admemes like admins possessing a bread
        return ev;
    }

    /// <summary>
    ///   Set CurrentLanguage of the client, the client must be able to Understand the language requested.
    /// </summary>
    private void OnClientSetLanguage(LanguagesSetMessage message, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not {Valid: true} speaker)
            return;

        var language = GetLanguagePrototype(message.CurrentLanguage);

        if (language == null || !CanSpeak(speaker, language.ID))
            return;

        SetLanguage(speaker, language.ID);
    }
    #endregion
}
