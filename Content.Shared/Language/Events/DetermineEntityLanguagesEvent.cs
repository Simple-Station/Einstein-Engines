using Robust.Shared.Prototypes;

namespace Content.Shared.Language.Events;

/// <summary>
///     Raised in order to determine the list of languages the entity can speak and understand at the given moment.
///     Typically raised on an entity after a language agent (e.g. a translator) has been added to or removed from them.
/// </summary>
[ByRefEvent]
public record struct DetermineEntityLanguagesEvent
{
    /// <summary>
    ///     The list of all languages the entity may speak.
    ///     By default, contains the languages this entity speaks intrinsically.
    /// </summary>
    public HashSet<ProtoId<LanguagePrototype>> SpokenLanguages = new();

    /// <summary>
    ///     The list of all languages the entity may understand.
    ///     By default, contains the languages this entity understands intrinsically.
    /// </summary>
    public HashSet<ProtoId<LanguagePrototype>> UnderstoodLanguages = new();

    public DetermineEntityLanguagesEvent() {}
}
