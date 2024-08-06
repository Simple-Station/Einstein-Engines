using Content.Shared.Language;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype.List;

namespace Content.Server.Traits.Assorted;

/// <summary>
///     Used for traits that modify entities' language knowledge.
/// </summary>
[RegisterComponent]
public sealed partial class LanguageKnowledgeModifierComponent : Component
{
    /// <summary>
    ///     List of languages this entity will learn to speak.
    /// </summary>
    [DataField("speaks")]
    public List<string> NewSpokenLanguages = new();

    /// <summary>
    ///     List of languages this entity will learn to understand.
    /// </summary>
    [DataField("understands")]
    public List<string> NewUnderstoodLanguages = new();
}
