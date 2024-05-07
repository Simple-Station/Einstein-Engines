using Content.Shared.Actions;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared.Language.Systems;

public abstract class SharedLanguageSystem : EntitySystem
{
    /// <summary>
    ///     The language used as a fallback in cases where an entity suddenly becomes a language speaker (e.g. the usage of make-sentient)
    /// </summary>
    [ValidatePrototypeId<LanguagePrototype>]
    public static readonly string FallbackLanguagePrototype = "GalacticCommon";
    /// <summary>
    ///     The language whose speakers are assumed to understand and speak every language. Should never be added directly.
    /// </summary>
    [ValidatePrototypeId<LanguagePrototype>]
    public static readonly string UniversalPrototype = "Universal";

    /// <summary>
    ///     A cached instance of <see cref="UniversalPrototype"/>
    /// </summary>
    public static LanguagePrototype Universal { get; private set; } = default!;

    [Dependency] protected readonly IPrototypeManager _prototype = default!;
    [Dependency] protected readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        Universal = _prototype.Index<LanguagePrototype>("Universal");
    }

    public LanguagePrototype? GetLanguage(string id)
    {
        _prototype.TryIndex<LanguagePrototype>(id, out var proto);
        return proto;
    }
}
