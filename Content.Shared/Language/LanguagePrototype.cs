using System.Runtime.CompilerServices;
using Robust.Shared.Prototypes;

namespace Content.Shared.Language;

[Prototype("language")]
public sealed class LanguagePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set;  } = default!;

    // <summary>
    // If true, obfuscated phrases of creatures speaking this language will have their syllables replaced with "replacement" syllables.
    // Otherwise entire sentences will be replaced.
    // </summary>
    [DataField(required: true)]
    public bool ObfuscateSyllables;

    // <summary>
    // Lists all syllables that are used to obfuscate a message a listener cannot understand if obfuscateSyllables is true,
    // Otherwise uses all possible phrases the creature can make when trying to say anything.
    // </summary>
    [DataField(required: true)]
    public List<string> Replacement = [];

    #region utility
    /// <summary>
    ///     The in-world name of this language, localized.
    /// </summary>
    public string Name => Loc.GetString($"language-{ID}-name");

    /// <summary>
    ///     The in-world description of this language, localized.
    /// </summary>
    public string Description => Loc.GetString($"language-{ID}-description");
    #endregion utility
}
