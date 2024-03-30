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

    /// <summary>
    ///   A random number added to each pseudo-random number's seed. Changes every round.
    /// </summary>
    public int RandomRoundSeed { get; private set; }

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LanguageSpeakerComponent, ComponentInit>(OnInitLanguageSpeaker);
        SubscribeLocalEvent<RoundStartingEvent>(_ => RandomRoundSeed = _random.Next());

        InitializeNet();
    }

    #region public api
    /// <summary>
    ///   Obfuscate the speech of the given entity using its default language.
    /// </summary>
    public string ObfuscateSpeech(EntityUid source, string message)
    {
        var language = GetLanguage(source) ?? Universal;
        return ObfuscateSpeech(message, language);
    }

    /// <summary>
    ///     Obfuscate a message using the given language.
    /// </summary>
    public string ObfuscateSpeech(string message, LanguagePrototype language)
    {
        var builder = new StringBuilder();
        if (language.ObfuscateSyllables)
        {
            ObfuscateSyllables(builder, message, language);
        }
        else
        {
            ObfuscatePhrases(builder, message, language);
        }

        return builder.ToString();
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

    // <summary>
    //     Returns the current language of the given entity. Assumes Universal if not specified.
    // </summary>
    public LanguagePrototype GetLanguage(EntityUid speaker, LanguageSpeakerComponent? languageComp = null)
    {
        var id = GetLanguages(speaker, languageComp)?.CurrentLanguage;
        if (id == null)
            return Universal; // Fallback

        _prototype.TryIndex(id, out LanguagePrototype? proto);

        return proto ?? Universal;
    }

    // <summary>
    //     Set the CurrentLanguage of the given entity.
    // </summary>
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

    /// <summary>
    ///   Returns a pair of (spoken, understood) languages of the given entity.
    /// </summary>
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
        {
            component.CurrentLanguage = component.SpokenLanguages.FirstOrDefault(UniversalPrototype);
        }
    }
    #endregion

    #region internal api - obfuscation
    private void ObfuscateSyllables(StringBuilder builder, string message, LanguagePrototype language)
    {
        // Go through each word. Calculate its hash sum and count the number of letters.
        // Replicate it with pseudo-random syllables of pseudo-random (but similar) length. Use the hash code as the seed.
        // This means that identical words will be obfuscated identically. Simple words like "hello" or "yes" in different langs can be memorized.
        var wordBeginIndex = 0;
        var hashCode = 0;
        for (var i = 0; i < message.Length; i++)
        {
            var ch = char.ToLower(message[i]);
            // A word ends when one of the following is found: a space, a sentence end, or EOM
            if (char.IsWhiteSpace(ch) || IsSentenceEnd(ch) || i == message.Length - 1)
            {
                var wordLength = i - wordBeginIndex;
                if (wordLength > 0)
                {
                    var newWordLength = PseudoRandomNumber(hashCode, 1, 4);

                    for (var j = 0; j < newWordLength; j++)
                    {
                        var index = PseudoRandomNumber(hashCode + j, 0, language.Replacement.Count);
                        builder.Append(language.Replacement[index]);
                    }
                }

                builder.Append(ch);
                hashCode = 0;
                wordBeginIndex = i + 1;
            }
            else
            {
                hashCode = hashCode * 31 + ch;
            }
        }
    }

    private void ObfuscatePhrases(StringBuilder builder, string message, LanguagePrototype language)
    {
        // In a similar manner, each phrase is obfuscated with a random number of conjoined obfuscation phrases.
        // However, the number of phrases depends on the number of characters in the original phrase.
        var sentenceBeginIndex = 0;
        for (var i = 0; i < message.Length; i++)
        {
            var ch = char.ToLower(message[i]);
            if (IsSentenceEnd(ch) || i == message.Length - 1)
            {
                var length = i - sentenceBeginIndex;
                if (length > 0)
                {
                    var newLength = (int) Math.Clamp(Math.Cbrt(length) - 1, 1, 4); // 27+ chars for 2 phrases, 64+ for 3, 125+ for 4.

                    for (var j = 0; j < newLength; j++)
                    {
                        var phrase = _random.Pick(language.Replacement);
                        builder.Append(phrase);
                    }
                }
                sentenceBeginIndex = i + 1;

                if (IsSentenceEnd(ch))
                    builder.Append(ch).Append(" ");
            }
        }
    }

    private static bool IsSentenceEnd(char ch)
    {
        return ch is '.' or '!' or '?';
    }
    #endregion

    #region internal api - misc
    /// <summary>
    ///   Dynamically resolves the current language of the entity and the list of all languages it speaks.
    ///   The returned event is reused and thus must not be held as a reference anywhere but inside the caller function.
    ///
    ///   If the entity is not a language speaker, or is a universal language speaker, then it's assumed to speak Universal,
    ///   aka all languages at once and none at the same time.
    /// </summary>
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
    ///   Generates a stable pseudo-random number in the range [min, max) for the given seed. Each input seed corresponds to exactly one random number.
    /// </summary>
    private int PseudoRandomNumber(int seed, int min, int max)
    {
        // This is not a uniform distribution, but it shouldn't matter: given there's 2^31 possible random numbers,
        // The bias of this function should be so tiny it will never be noticed.
        seed += RandomRoundSeed;
        var random = ((seed * 1103515245) + 12345) & 0x7fffffff; // Source: http://cs.uccs.edu/~cs591/bufferOverflow/glibc-2.2.4/stdlib/random_r.c
        return random % (max - min) + min;
    }
    #endregion
}
