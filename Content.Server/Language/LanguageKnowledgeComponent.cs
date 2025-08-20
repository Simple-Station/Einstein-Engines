using System.ComponentModel.DataAnnotations;
using Content.Shared.Language;
using Robust.Shared.Prototypes;

namespace Content.Server.Language;

/// <summary>
///     Stores data about entities' intrinsic language knowledge. Setting this OVERRIDES the entity's known languages.
/// </summary>
[RegisterComponent]
public sealed partial class LanguageKnowledgeComponent : Component
{
    /// <summary>
    ///     List of languages this entity can speak without any external tools.
    /// </summary>
    [DataField("speaks", required: true)]
    public List<ProtoId<LanguagePrototype>> SpokenLanguages = new();

    /// <summary>
    ///     List of languages this entity can understand without any external tools.
    /// </summary>
    [DataField("understands", required: true)]
    public List<ProtoId<LanguagePrototype>> UnderstoodLanguages = new();

}
