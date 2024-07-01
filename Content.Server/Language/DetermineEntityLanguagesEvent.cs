namespace Content.Server.Language;

/// <summary>
///   Raised in order to determine the language an entity speaks at the current moment,
///   as well as the list of all languages the entity may speak and understand.
/// </summary>
public sealed class DetermineEntityLanguagesEvent : EntityEventArgs
{
    /// <summary>
    ///   The default language of this entity. If empty, remain unchanged.
    ///   This field has no effect if the entity decides to speak in a concrete language.
    /// </summary>
    public string CurrentLanguage;
    /// <summary>
    ///   The list of all languages the entity may speak. Must NOT be held as a reference!
    /// </summary>
    public List<string> SpokenLanguages;
    /// <summary>
    ///   The list of all languages the entity may understand. Must NOT be held as a reference!
    /// </summary>
    public List<string> UnderstoodLanguages;

    public DetermineEntityLanguagesEvent(string currentLanguage, List<string> spokenLanguages, List<string> understoodLanguages)
    {
        CurrentLanguage = currentLanguage;
        SpokenLanguages = spokenLanguages;
        UnderstoodLanguages = understoodLanguages;
    }
}
